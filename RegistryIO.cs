using Microsoft.Win32;
using System;

class RegistryIO
{
	private static readonly string RegistrySubKeyPath = "Software\\MikeCAT\\TrainVoiceGuide";

	private RegistryKey Key;

	private RegistryIO(RegistryKey key)
	{
		Key = key;
	}

	public static RegistryIO OpenForRead()
	{
		try
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistrySubKeyPath, false);
			if (key == null) return null;
			return new RegistryIO(key);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public static RegistryIO OpenForWrite()
	{
		try
		{
			RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistrySubKeyPath);
			if (key == null) return null;
			return new RegistryIO(key);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public string GetStringValue(string name)
	{
		try
		{
			object data = Key.GetValue(name);
			if (data is string)
			{
				return (string)data;
			}
			else
			{
				return null;
			}
		}
		catch (Exception)
		{
			return null;
		}
	}

	public int? GetIntValue(string name)
	{
		try
		{
			object data = Key.GetValue(name);
			if (data is int && (int)data >= 0)
			{
				return (int)data;
			}
			else
			{
				return null;
			}
		}
		catch (Exception)
		{
			return null;
		}
	}

	public void SetValue(string name, object value)
	{
		try
		{
			Key.SetValue(name, value);
		}
		catch (Exception)
		{
			// 握りつぶす
		}
	}

	public void Close()
	{
		Key.Close();
	}
}
