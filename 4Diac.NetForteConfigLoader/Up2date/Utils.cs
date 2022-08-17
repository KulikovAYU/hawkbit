namespace Up2date
{
    public class Utils
    {
        public static string HawkbitEndpointFrom(string endPoint, string controllerId, string tenant)
        {
            var hawkbitEndPoint = new Uri(endPoint);
            var endPt  = $"{hawkbitEndPoint.Scheme}://{hawkbitEndPoint.Authority}/{tenant}/controller/v1/{controllerId}";

            return endPt;
        }
    }
}