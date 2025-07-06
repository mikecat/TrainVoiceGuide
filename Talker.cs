using System;
using System.Runtime.InteropServices;

class Talker: IDisposable
{
	[DllImport("AquesTalkDa.dll", ExactSpelling = true)]
	private static extern IntPtr AquesTalkDa_Create();

	[DllImport("AquesTalkDa.dll", ExactSpelling = true)]
	private static extern void AquesTalkDa_Release(IntPtr hMe);

	[DllImport("AquesTalkDa.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
	private static extern int AquesTalkDa_Play(IntPtr hMe, string koe, int iSpeed, IntPtr hWnd, uint msg, uint dwUser);

	[DllImport("AquesTalkDa.dll", ExactSpelling = true)]
	private static extern void AquesTalkDa_Stop(IntPtr hMe);

	[DllImport("AquesTalkDa.dll", ExactSpelling = true)]
	private static extern int AquesTalkDa_IsPlay(IntPtr hMe);

	public enum PlayType
	{
		IgnoreIfPlaying,
		OverrideIfPlaying,
		QueueIfPlaying,
	}

	private IntPtr synthInstance;

	public Talker()
	{
		synthInstance = AquesTalkDa_Create();
	}

	public void Dispose()
	{
		if (synthInstance != IntPtr.Zero)
		{
			AquesTalkDa_Release(synthInstance);
			synthInstance = IntPtr.Zero;
		}
	}

	public bool Playing
	{
		get
		{
			return AquesTalkDa_IsPlay(synthInstance) != 0;
		}
	}

	public bool Play(string voiceString, int speed, PlayType playType, IntPtr hWnd, uint msg)
	{
		if (synthInstance == IntPtr.Zero) throw new ObjectDisposedException("cannot play after disposed");
		if (this.Playing)
		{
			if (playType == PlayType.IgnoreIfPlaying) return false;
			else if (playType == PlayType.OverrideIfPlaying) AquesTalkDa_Stop(synthInstance);
		}
		AquesTalkDa_Play(synthInstance, voiceString, speed, hWnd, msg, 0);
		return true;
	}

	public bool Play(string voiceString, int speed, PlayType playType)
	{
		return Play(voiceString, speed, playType, IntPtr.Zero, 0);
	}
}
