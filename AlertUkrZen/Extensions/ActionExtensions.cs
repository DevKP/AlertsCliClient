namespace AlertUkrZen.Extensions
{
    public static class ActionExtensions
    {
        public static bool IsNoneAction(this Action action)
        {
            return action == default;
        }
        public static bool IsQuitAction(this Action action)
        {
            return action == Action.Quit;
        }

        public static bool IsAutoUpdateAction(this Action action)
        {
            return action == Action.AutoUpdate;
        }

        public static bool IsToggleLastAlerts(this Action action)
        {
            return action == Action.ToggleLastAlerts;
        }

        public static bool IsToggleActiveAlerts(this Action action)
        {
            return action == Action.ToggleActiveAlerts;
        }
    }
}