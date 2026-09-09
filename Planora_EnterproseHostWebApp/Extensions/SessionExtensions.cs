using Microsoft.AspNetCore.Http;

namespace Planora_EnterproseHostWebApp.Extensions
{
    /// <summary>
    /// Single source of truth for the current event's public/private type while
    /// a host is going through the Create Event wizard.
    ///
    /// Before this, the choice made on Step 1 only ever lived in the URL
    /// (?type=private), rewritten by client-side JS on every wizard link.
    /// Any server-side RedirectToPage() call drops query strings unless the
    /// code explicitly re-adds them, so the value silently reverted to
    /// "public" partway through the wizard. Some pages also read a session
    /// key ("IsPrivateEvent") that nothing ever wrote, which made those
    /// checks always false.
    ///
    /// Everything in the wizard should now go through GetEventType() /
    /// SetEventType() / IsPrivateEvent() instead of the query string or the
    /// old "IsPrivateEvent" key.
    /// </summary>
    public static class SessionExtensions
    {
        private const string EventTypeKey = "EventType";

        public static void SetEventType(this ISession session, string eventType)
        {
            var normalized = string.Equals(eventType, "private", System.StringComparison.OrdinalIgnoreCase)
                ? "private"
                : "public";

            session.SetString(EventTypeKey, normalized);
        }

        public static string GetEventType(this ISession session)
        {
            var value = session.GetString(EventTypeKey);
            return string.Equals(value, "private", System.StringComparison.OrdinalIgnoreCase)
                ? "private"
                : "public";
        }

        public static bool IsPrivateEvent(this ISession session)
        {
            return session.GetEventType() == "private";
        }
    }
}
