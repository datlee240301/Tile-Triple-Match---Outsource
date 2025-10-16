// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("yVPPLjbwwtdv8ms57UVwJSfH5zVbRIQ+1rFcsmmXe0u3eCbX1yJoQ/MXrErpWLn0+rwgS+llc+NexKqlOLSl0H7dB2lBBqs1UHDlxWn4e74gkhEyIB0WGTqWWJbnHRERERUQE3X902dA3jwp/w224ettAH78psXt5S19zehXNMvbAoGynZ9hj2g0HiWSER8QIJIRGhKSEREQ3985wVOpL/+31la8j6nn2QMIC14tcpjvjdhX7Yumn2s/4RElfhnxfcaLxuKz0FYxvULGq3IsyF31cnADWxpF4hYsDnN623Eid2yQLqqD9VRrY1VE7xoIqE3wHRXuDmNSAJDyFOlKZKkSm8cVFgQDQd2YZR9BGv0OF02rkQUsXe3AnrPKX2+9CxITERAR");
        private static int[] order = new int[] { 8,5,4,9,8,6,10,10,8,13,13,12,13,13,14 };
        private static int key = 16;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
