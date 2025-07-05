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

	public void Play(string voiceString, int speed, PlayType playType)
	{
		if (synthInstance == IntPtr.Zero) throw new ObjectDisposedException("cannot play after disposed");
		if (AquesTalkDa_IsPlay(synthInstance) != 0)
		{
			if (playType == PlayType.IgnoreIfPlaying) return;
			else if (playType == PlayType.OverrideIfPlaying) AquesTalkDa_Stop(synthInstance);
		}
		AquesTalkDa_Play(synthInstance, voiceString, speed, IntPtr.Zero, 0, 0);
	}
}
