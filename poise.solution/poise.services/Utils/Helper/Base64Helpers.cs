namespace poise.services.Utils.Helper;

public class Base64Helpers
{
	public static string Base64Decode(string base64EncodedData)
	{
		var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
		return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
	}

	public static string Base64Encode(string plainText)
	{
		var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
		return System.Convert.ToBase64String(plainTextBytes);
	}
}
