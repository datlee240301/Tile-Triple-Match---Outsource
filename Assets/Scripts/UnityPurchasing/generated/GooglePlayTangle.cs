// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("6HLuDxfR4/ZO00oYzGRRBAbmxhR6ZaUf95B9k0i2WmqWWQf29gNJYjQ3JSJg/LlEPmA73C82bIqwJA18VNzyRmH/HQjeLJfAykwhX92H5MwZlYTxX/wmSGAnihRxUcTkSNlan8QMXOzJdhXq+iOgk7y+QK5JFT8EszA+MQGzMDszszAwMf7+GOByiA5SW/pQA1ZNsQ+LotR1SkJ0Zc47Kd6W93edrojG+CIpKn8MU7nOrPl20jaNa8h5mNXbnQFqyERSwn/li4QBszATATw3OBu3ebfGPDAwMDQxMhCcY+eKUw3pfNRTUSJ6O2TDNw0vzKqHvkoewDAEXzjQXOeq58OS8XeJbNE8NM8vQnMhsdM1yGtFiDO65szhv5Lrfk6cKjMyMDEw");
        private static int[] order = new int[] { 8,5,11,6,9,10,10,12,11,9,11,11,13,13,14 };
        private static int key = 49;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
