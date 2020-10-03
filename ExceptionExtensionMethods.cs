using System;
using System.Text;

public static class ExceptionExtensionMethods
{
	public static string ToStringFull(this Exception ex)
	{
		if (ex != null)
		{
			return GetString(ex);
		}
		return "";
	}

	private static string GetString(Exception ex)
	{
		StringBuilder stringBuilder = new StringBuilder();
		GetStringRecursive(ex, stringBuilder);
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Stack trace:");
		stringBuilder.AppendLine(ex.StackTrace);
		return stringBuilder.ToString();
	}

	private static void GetStringRecursive(Exception ex, StringBuilder sb)
	{
		sb.AppendLine(ex.GetType().Name + ":");
		sb.AppendLine(ex.Message);
		if (ex.InnerException != null)
		{
			sb.AppendLine();
			GetStringRecursive(ex.InnerException, sb);
		}
	}
}
