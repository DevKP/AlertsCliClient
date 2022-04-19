using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AlertUkrZen.Extensions;
using AlertUkrZen.Models;
using Humanizer;

namespace AlertUkrZen
{
    internal class Program
    {
        private const ConsoleKey QuitKey = ConsoleKey.Q;
        private const ConsoleKey AutoUpdateKey = ConsoleKey.U;
        private const ConsoleKey AbortedKey = ConsoleKey.S;
        private const ConsoleKey ActiveKey = ConsoleKey.A;

        private const string QuitKeySymbol = nameof(ConsoleKey.Q);
        private const string AutoUpdateKeySymbol = nameof(ConsoleKey.U);
        private const string AbortedKeySymbol = nameof(ConsoleKey.S);
        private const string ActiveKeySymbol = nameof(ConsoleKey.A);

        private const string RegionToMonitor = "Франківська";
        private const string AlertStateOn = "Alert";
        private const string AlertStateOff = "No alert";
        private const string ActiveApiUrl = "http://142.93.128.30:8080/Alerts/Active";
        private const string LastApiUrl = "http://142.93.128.30:8080/Alerts/LastHours/";
        private const int LastHours = 2;
        private const int UpdateMillisecondsDelay = 1000;

        private static readonly HttpClient Client;
        private static bool _lastAlertsTurnedOn;
        private static bool _activeAlertsTurnedOn;
        private static bool _autoUpdateState;

        static Program()
        {
            Client = new HttpClient();
        }

        private static async Task Main()
        {
            EnableConsoleUtf8();
            await RunMainLoopAsync();
        }

        private static async Task RunMainLoopAsync()
        {
            while (await ProgramIsNotCompleted()) { }
        }

        private static async Task<bool> ProgramIsNotCompleted()
        {
            return !await RunToCompletion();
        }

        private static async Task<bool> RunToCompletion()
        {
            try
            {
                //Console.CursorLeft = 0;
                //Console.CursorTop = 0;

                var alerts = await GetAlertsAsync(ActiveApiUrl);
                var lastAlerts = await GetAlertsAsync($"{LastApiUrl}{LastHours}");

                Console.Clear();

                ShowSectionDelimeter();

                if (_activeAlertsTurnedOn)
                {
                    ShowAlertsList(alerts);
                    ShowSectionDelimeter();
                }

                if(_lastAlertsTurnedOn)
                    ShowLastAlertsList(lastAlerts);

                ShowMainRegionStatus(alerts);

                ShowSectionDelimeter();

                ShowLastUpdateTime();

                var quitRequested = StartDialogWithUser();

                await IfAutoUpdateAsync();
                
                return quitRequested;
            }
            catch (AlertException ex)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(UpdateMillisecondsDelay);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}\nStacktrace: {ex.StackTrace}");
                await Task.Delay(3000);
                Console.Read();
            }

            return false;
        }

        private static void ShowSectionDelimeter()
        {
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();
        }

        private static async Task IfAutoUpdateAsync()
        {
            if (_autoUpdateState)
            {
                throw new AlertException("Update in 1sec...");

                Console.WriteLine("\nAuto update is on. Press '{0}' to turn it off.", AutoUpdateKeySymbol);
                await Task.Delay(UpdateMillisecondsDelay);
            }                
        }

        //TODO: Refactor to single responsibility
        private static bool StartDialogWithUser()
        {
            ShowDialogMessage();
            var action = GetActionFromUser();

            if (action.IsAutoUpdateAction())
            {
                EnableAutoUpdate();
                return false;
            }

            if (action.IsToggleLastAlerts())
            {
                _lastAlertsTurnedOn = !_lastAlertsTurnedOn;
            }

            if (action.IsToggleActiveAlerts())
            {
                _activeAlertsTurnedOn = !_activeAlertsTurnedOn;
            }

            if (action.IsQuitAction())
                return true;

            return false;
        }

        private static void EnableAutoUpdate()
        {
            _autoUpdateState = true;
        }


        private static void ShowLastUpdateTime()
        {
            Console.WriteLine($"Last updated: {DateTime.Now:HH:mm:ss}");
        }

        private static void ShowMainRegionStatus(AlertsModel alertsModel)
        {
            var isAlert = IsAlertInMonitoredRegion(alertsModel);
            //Console.Write(Environment.NewLine);
            //Console.WriteLine("------");
            //Console.WriteLine($"{(isAlert ? AlertStateOn : AlertStateOff)} in {RegionToMonitor} обл.");
            //Console.WriteLine("------");
            //Console.Write(Environment.NewLine);

            var table = new Table(1,
                new[] {new[] {$"{(isAlert ? AlertStateOn : AlertStateOff)} in {RegionToMonitor} обл."}}, new []{"Тривога в конкретній області"});

            Console.WriteLine(table);
        }

        private static bool IsAlertInMonitoredRegion(AlertsModel alertsModel)
        {
            return alertsModel.Alerts.Any(a => a.LocationTitle.Contains(RegionToMonitor));
        }

        private static async Task<AlertsModel> GetAlertsAsync(string apiUrl)
        {
            var response = await GetResponseAsync(apiUrl);
            var alerts = await DeserializeResponseAsync(response);
            return alerts;
        }

        private static async Task<AlertsModel> GetAlertsFromFileAsync()
        {
            var fileText =
                await File.ReadAllTextAsync(
                    @"C:\Users\Vladyslav_Ilchenko\source\repos\TgAlarmsFetcher\TgAlarmsFetcher\bin\Debug\net6.0\alerts.json");
            var alerts = DeserializeText(fileText);
            return alerts;
        }

        private static void ShowDialogMessage()
        {
            Console.WriteLine($"Press any button to update.\n" +
                              $"{QuitKeySymbol} to quit.\n" +
                              $"{AutoUpdateKeySymbol} to auto update.\n" +
                              $"{ActiveKeySymbol} to toggle active alerts.\n" +
                              $"{AbortedKeySymbol} to toggle aborted alerts.");
        }


        //Don't check if key available if autoupdate is off
        //Otherwise don't block if key not pressed
        private static Action GetActionFromUser()
        {
            if (_autoUpdateState && !Console.KeyAvailable)
                return Action.None;

            var key = Console.ReadKey(true).Key;
            return GetActionFromKey(key);
        }

        private static Action GetActionFromKey(ConsoleKey key)
        {
            if (key == QuitKey)
                return Action.Quit;

            if (key == AutoUpdateKey)
                return Action.AutoUpdate;

            if (key == ActiveKey)
                return Action.ToggleActiveAlerts;

            if (key == AbortedKey)
                return Action.ToggleLastAlerts;

            return Action.None;
        }

        private static void ShowAlertsList(AlertsModel alertsModel)
        {
            var rows = AlertsToTable(alertsModel).ToList();

            Console.WriteLine("[Активна тривога]");
            if (!rows.Any())
            {
                Console.WriteLine(new Table(1, new[] {new[] {"Все тихо"}}, new[] {"Все тихо"}));
                return;
            }

            Console.WriteLine(new Table(3, rows, new[] { "Місце", "Початок", "Тривалість" }));
        }

        private static void ShowLastAlertsList(AlertsModel alertsModel)
        {
            var rows = LastAlertsToTable(alertsModel).ToList();

            Console.WriteLine($"[Відбій за останні {LastHours} години]");
            if (!rows.Any())
            {
                Console.WriteLine(new Table(1, new[] { new[] { "Пусто" } }, new[] { "Пусто" }));
                return;
            }

            Console.WriteLine(new Table(3, rows, new[] { "Місце", "Кінець", "Тривалість" }));
        }

        private static IEnumerable<string[]> AlertsToTable(AlertsModel alertsModel)
        {
            return alertsModel.Alerts.OrderBy(alert => alert.Duration).Select(alert => 
                new[]
                {
                    alert.LocationTitle,
                    $"{alert.StartedAt.ToLocalTime():dd.MM.yyyy HH:mm:ss}",
                    alert.Duration.Humanize(2, CultureInfo.GetCultureInfo("uk-UA"))
                });
        }

        private static IEnumerable<string[]> LastAlertsToTable(AlertsModel alertsModel)
        {
            return alertsModel.Alerts.OrderBy(alert => alert.Duration).Select(alert =>
                new[]
                {
                    alert.LocationTitle,
                    $"{alert.EndedAt.ToLocalTime():dd.MM.yyyy HH:mm:ss}",
                    alert.Duration.Humanize(2, CultureInfo.GetCultureInfo("uk-UA"))
                });
        }

        private static async Task<AlertsModel> DeserializeResponseAsync(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var alerts = DeserializeText(responseString);

            return alerts;
        }

        private static AlertsModel DeserializeText(string json)
        {
            var alerts = JsonSerializer.Deserialize<AlertsModel>(json)
                         ?? throw new AlertException("Deserialization error!");

            return alerts;
        }

        private static async Task<HttpResponseMessage> GetResponseAsync(string apiUrl)
        {
            //Console.WriteLine("Requesting active.json");

            var response = await Client.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new AlertException($"Request error! Code: {response.StatusCode}");
            }

            return response;
        }

        private static void EnableConsoleUtf8()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
    }
}
