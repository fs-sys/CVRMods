using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using ABI_RC.Core.Player;
using UnityEngine;
using static UnityEngine.Mathf;

namespace VRCPlates;

//Thank you Bono for letting me use the random color code.
public static class Utils
{
	private static readonly MD5 Hasher = MD5.Create();

	private static int Combine(this byte b1, byte concat)
	{
		var combined = b1 << 8 | concat;
		return combined;
	}

	public static Color GetColourFromUserID(string userID)
	{
		var hash = Hasher.ComputeHash(Encoding.UTF8.GetBytes(userID));
		var colour2 = hash[3].Combine(hash[4]);
		//Fixed saturation and brightness values, only hue is altered
		return Color.HSVToRGB(colour2 / 65535f, .8f, .8f);
	}

	public static Color GetColorForSocialRank(string playerApiUserRank) =>
		playerApiUserRank switch
		{
			"User" => new Color32(0, 183, 36, 255),
			"Legend" => new Color32(50, 150, 147, 255),
			"Community Guide" => new Color32(221, 90, 0, 255),
			"Moderator" => new Color32(221, 0, 118, 255),
			"Developer" => new Color32(240, 0, 40, 255),
			_ => new Color32(0, 183, 36, 255)
		};

	public static string? GetAbbreviation(string playerApiUserRank) => playerApiUserRank switch
	{
		"Community Guide" => "GD",
		"Moderator" => "MOD",
		"Developer" => "DEV",
		_ => null
	};

	public static CVRPlayerEntity? GetPlayerEntity(string? userID)
	{
		var player = CVRPlayerManager.Instance.NetworkPlayers.Find(p=> p.Uuid == userID);
		if (player != null)
		{
			return player;
		}
		VRCPlates.Error("[0027] Could not find player entity for user ID: " + userID);
		return null;
	}
}

[Serializable]
[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
public struct HsbColor
{
	public float h;

	public float s;

	public float b;

	public float a;

	public HsbColor(float h, float s, float b, float a)
	{
		this.h = h;
		this.s = s;
		this.b = b;
		this.a = a;
	}

	public HsbColor(float h, float s, float b)
	{
		this.h = h;
		this.s = s;
		this.b = b;
		a = 1f;
	}

	public HsbColor(Color col)
	{
		var hSbColor = FromColor(col);
		h = hSbColor.h;
		s = hSbColor.s;
		b = hSbColor.b;
		a = hSbColor.a;
	}

	public static HsbColor FromColor(Color color)
	{
		var result = new HsbColor(0f, 0f, 0f, color.a);
		var r = color.r;
		var g = color.g;
		var num = color.b;
		var num2 = Max(r, Max(g, num));
		if (num2 <= 0f)
		{
			return result;
		}

		var num3 = Min(r, Min(g, num));
		var num4 = num2 - num3;
		if (num2 > num3)
		{
			if (g == num2)
			{
				result.h = (num - r) / num4 * 60f + 120f;
			}
			else if (num == num2)
			{
				result.h = (r - g) / num4 * 60f + 240f;
			}
			else if (num > g)
			{
				result.h = (g - num) / num4 * 60f + 360f;
			}
			else
			{
				result.h = (g - num) / num4 * 60f;
			}

			if (result.h < 0f)
			{
				result.h += 360f;
			}
		}
		else
		{
			result.h = 0f;
		}

		result.h *= 0.00277777785f;
		result.s = num4 / num2 * 1f;
		result.b = num2;
		return result;
	}

	public static Color ToColor(HsbColor hsbColor)
	{
		var value = hsbColor.b;
		var value2 = hsbColor.b;
		var value3 = hsbColor.b;
		if (hsbColor.s == 0f) return new Color(Clamp01(value), Clamp01(value2), Clamp01(value3), hsbColor.a);
		var num = hsbColor.b;
		var num2 = hsbColor.b * hsbColor.s;
		var num3 = hsbColor.b - num2;
		var num4 = hsbColor.h * 360f;
		switch (num4)
		{
			case < 60f:
				value = num;
				value2 = num4 * num2 / 60f + num3;
				value3 = num3;
				break;
			case < 120f:
				value = (0f - (num4 - 120f)) * num2 / 60f + num3;
				value2 = num;
				value3 = num3;
				break;
			case < 180f:
				value = num3;
				value2 = num;
				value3 = (num4 - 120f) * num2 / 60f + num3;
				break;
			case < 240f:
				value = num3;
				value2 = (0f - (num4 - 240f)) * num2 / 60f + num3;
				value3 = num;
				break;
			case < 300f:
				value = (num4 - 240f) * num2 / 60f + num3;
				value2 = num3;
				value3 = num;
				break;
			case <= 360f:
				value = num;
				value2 = num3;
				value3 = (0f - (num4 - 360f)) * num2 / 60f + num3;
				break;
			default:
				value = 0f;
				value2 = 0f;
				value3 = 0f;
				break;
		}

		return new Color(Clamp01(value), Clamp01(value2), Clamp01(value3), hsbColor.a);
	}

	public Color ToColor() => ToColor(this);

	public override string ToString() => "H:" + h + " S:" + s + " B:" + b;

	public static HsbColor Lerp(HsbColor a, HsbColor b, float t)
	{
		float num;
		float num2;
		if (a.b == 0f)
		{
			num = b.h;
			num2 = b.s;
		}
		else if (b.b == 0f)
		{
			num = a.h;
			num2 = a.s;
		}
		else
		{
			if (a.s == 0f)
			{
				num = b.h;
			}
			else if (b.s == 0f)
			{
				num = a.h;
			}
			else
			{
				float num3;
				for (num3 = LerpAngle(a.h * 360f, b.h * 360f, t); num3 < 0f; num3 += 360f)
				{
				}

				while (num3 > 360f)
				{
					num3 -= 360f;
				}

				num = num3 / 360f;
			}

			num2 = Mathf.Lerp(a.s, b.s, t);
		}

		return new HsbColor(num, num2, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
	}
}