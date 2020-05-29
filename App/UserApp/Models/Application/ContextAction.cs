namespace Intersoft.CISSA.UserApp.Models.Application
{
    public class ContextAction
    {
        public string ControllerName { get; private set; }
        public string ActionName { get; private set; }
        public object RouteValues { get; private set; }

        public ContextAction(string controllerName, string actionName) : this(controllerName, actionName, null) {}

        public ContextAction(string controllerName, string actionName, object routeValues)
        {
            ControllerName = controllerName;
            ActionName = actionName;
            RouteValues = routeValues;
        }
    }
}