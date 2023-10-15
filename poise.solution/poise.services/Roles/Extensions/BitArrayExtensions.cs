using System.Collections;
using System.Text;

namespace poise.services.Roles.Extensions;

public static class BitArrayExtensions
{
	public static string ToBitArrayString(this BitArray array)
	{
		var sb = new StringBuilder();
		for (var i = 0; i < array.Count; i++)
		{
			sb.Append(array[i] ? "1" : "0");
		}

		return sb.ToString();
	}

	public static BitArray ToBitArray(this string array)
	{
		var bitArray = new BitArray(100, false);
		for (var i = 0; i < array.Length; i++)
		{
			bitArray.Set(i, array[i] == '1');
		}

		return bitArray;
	}
}