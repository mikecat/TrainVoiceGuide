using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using TrainCrew;

class TrainVoiceGuide: Form
{
	public static void Main()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		Application.Run(new TrainVoiceGuide());
	}

	private static string DistanceToVoiceString(float distance, bool useCentimeter = false)
	{
		if (useCentimeter && Math.Abs(distance) <= 3)
		{
			int value = (int)Math.Round(distance * 100);
			if (value < 0)
				return string.Format("まいなす<NUMK VAL={0} COUNTER=せん'ち+め'ーとる>", -value);
			else
				return string.Format("<NUMK VAL={0} COUNTER=せん'ち+め'ーとる>", value);
		}
		else
		{
			int value = (int)Math.Round(distance);
			if (value < 0)
				return string.Format("まいなす<NUMK VAL={0} COUNTER=め'ーとる>", -value);
			else
				return string.Format("<NUMK VAL={0} COUNTER=め'ーとる>", value);
		}
	}

	private static string TimeToVoiceString(float time)
	{
		int value = (int)Math.Ceiling(time);
		if (value < 0)
			return string.Format("まいなす<NUMK VAL={0} COUNTER=びょう>", -value);
		else
			return string.Format("<NUMK VAL={0} COUNTER=びょう>", value);
	}

	private static string LimitToVoiceString(float limit)
	{
		if (limit < 0)
		{
			return "せいげ'ん/な'し";
		}
		else
		{
			int value = (int)Math.Round(limit);
			return string.Format("せいげ'ん/<NUMK VAL={0}>", value);
		}
	}

	private static string NoticeToVoiceString(float limit)
	{
		if (limit < 0)
		{
			return "よこく/な'し";
		}
		else
		{
			int value = (int)Math.Round(limit);
			return string.Format("よこく/<NUMK VAL={0}>", value);
		}
	}

	private const int fontSize = 16, gridSize = 24;

	private static Size GetSizeOnGrid(float width, float height)
	{
		return new Size((int)(gridSize * width), (int)(gridSize * height));
	}

	private static Point GetPointOnGrid(float x, float y)
	{
		return new Point((int)(gridSize * x), (int)(gridSize * y));
	}

	private static T CreateControl<T>(Control parent, float x, float y, float width, float height)
	where T: Control, new()
	{
		T control = new T();
		control.Location = GetPointOnGrid(x, y);
		control.Size = GetSizeOnGrid(width, height);
		if (parent != null) parent.Controls.Add(control);
		return control;
	}

	private readonly static string DistanceTimeTypeValueName = "distanceTimeType";
	private readonly static string DistanceTimeTypeNoneData = "none";
	private readonly static string DistanceTimeTypeTimeData = "time";
	private readonly static string DistanceTimeTypeDistanceData = "distance";
	private readonly static string DistanceTimeTypeTimeDistanceData = "timeDistance";
	private readonly static string DistanceTimeTypeDistanceTimeData = "distanceTime";

	private readonly static string DistanceTimeSpeedValueName = "distanceTimeSpeed";

	private readonly static string DistainceTimeTriggerStartValueName = "distanceTimeTriggerStart";
	private readonly static string DistainceTimeTriggerStopValueName = "distanceTimeTriggerStop";
	private readonly static string DistainceTimeTriggerNextStationChangeValueName = "distanceTimeTriggerNextStationChange";
	private readonly static string DistainceTimeTriggerDistanceValueName = "distanceTimeTriggerDistance";
	private readonly static string DistainceTimeTriggerTimeValueName = "distanceTimeTriggerTime";
	private readonly static string DistainceTimeTriggerDistanceIntervalValueName = "distanceTimeTriggerDistanceInterval";
	private readonly static string DistainceTimeTriggerTimeIntervalValueName = "distanceTimeTriggerTimeInterval";

	private readonly static string SpeedTypeValueName = "speedType";
	private readonly static string SpeedTypeNoneData = "none";
	private readonly static string SpeedTypeLimitData = "limit";
	private readonly static string SpeedTypeNoticeData = "notice";
	private readonly static string SpeedTypeLimitNoticeData = "limitNotice";
	private readonly static string SpeedTypeNoticeLimitData = "noticeLimit";

	private readonly static string SpeedSpeedValueName = "speedSpeed";

	private GroupBox distanceTimeGroup;
	private Label distanceTimeEnableLabel;
	private Panel distanceTimeEnablePanel;
	private RadioButton distanceTimeEnableNoneRadio;
	private RadioButton distanceTimeEnableTimeRadio;
	private RadioButton distanceTimeEnableDistanceRadio;
	private RadioButton distanceTimeEnableTimeDistanceRadio;
	private RadioButton distanceTimeEnableDistanceTimeRadio;
	private Label distanceTimeSpeedLabel;
	private HScrollBar distanceTimeSpeedScroll;
	private NumericUpDown distanceTimeSpeedInput;
	private Button distanceTimeSpeedTestButton;
	private GroupBox distanceTimeTriggerGroup;
	private CheckBox distanceTimeTriggerStartCheck;
	private CheckBox distanceTimeTriggerStopCheck;
	private CheckBox distanceTimeTriggerNextStationChangeCheck;
	private CheckBox distanceTimeTriggerDistanceCheck;
	private CheckBox distanceTimeTriggerTimeCheck;
	private NumericUpDown distanceTimeTriggerDistanceInput;
	private NumericUpDown distanceTimeTriggerTimeInput;
	private Label distanceTimeTriggerDistanceLabel;
	private Label distanceTimeTriggerTimeLabel;

	private GroupBox speedGroup;
	private Label speedEnableLabel;
	private Panel speedEnablePanel;
	private RadioButton speedNoneRadio;
	private RadioButton speedLimitRadio;
	private RadioButton speedNoticeRadio;
	private RadioButton speedLimitNoticeRadio;
	private RadioButton speedNoticeLimitRadio;
	private Label speedSpeedLabel;
	private HScrollBar speedSpeedScroll;
	private NumericUpDown speedSpeedInput;
	private Button speedSpeedTestButton;

	private Panel statusPanel;
	private Label statusNextStationTitleLabel;
	private Label statusNextStationLabel;
	private Label statusDistanceTitleLabel;
	private Label statusDistanceLabel;
	private Label statusTimeTitleLabel;
	private Label statusTimeLabel;
	private Label statusSpeedTitleLabel;
	private Label statusSpeedLabel;
	private Label statusLimitTitleLabel;
	private Label statusLimitLabel;
	private Label statusLimitNoticeTitleLabel;
	private Label statusLimitNoticeLabel;

	private struct VoiceData
	{
		public string voiceString;
		public int speed;
		public bool uniqueInQueue;
	}

	private Timer timer;
	private Talker testTalker = new Talker();
	private Talker infoTalker = new Talker();
	private List<VoiceData> infoQueue = new List<VoiceData>();
	private const uint WM_APP = 0x8000;

	private int? prevNextStationIndex = null;
	private float? prevNextStationDistance = null;
	private float? prevNextStationTime = null;
	private float? prevSpeed = null;
	private float? prevSpeedLimit = null;
	private float? prevSpeedLimitNotice = null;

	private void AddVoiceToQueue(string voiceString, int speed, bool uniqueInQueue)
	{
		if (!uniqueInQueue || !infoQueue.Exists((e) => e.uniqueInQueue))
		{
			infoQueue.Add(
				new VoiceData() {
					voiceString = voiceString,
					speed = speed,
					uniqueInQueue = uniqueInQueue,
				}
			);
			if (infoQueue.Count == 1)
			{
				// 外部から勝手にメッセージが送られても、おかしくなりにくくする
				// 外部メッセージの影響で再生中なのにキューが空でも、再生できるようにする
				infoTalker.Play(voiceString, speed, Talker.PlayType.QueueIfPlaying, this.Handle, WM_APP);
			}
		}
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
		if (m.Msg == WM_APP)
		{
			// 再生が終わったらしい (もしくは、外部から勝手にメッセージが送られた)
			if (infoQueue.Count <= 1)
			{
				// 再生が終わったのはキュー内最後のデータ
				if (!infoTalker.Playing)
				{
					// 本当に再生が終わっているならば、デキューする
					if (infoQueue.Count > 0) infoQueue.RemoveAt(0);
				}
			}
			else
			{
				// キューにまだデータがある
				// 再生中なら再生しないモードで、再生を試みる
				bool played = infoTalker.Play(infoQueue[1].voiceString, infoQueue[1].speed, Talker.PlayType.IgnoreIfPlaying, this.Handle, WM_APP);
				// 再生できた (すなわち、再生が終わっていた) なら、デキューする
				if (played)infoQueue.RemoveAt(0);
			}
		}
	}

	public TrainVoiceGuide()
	{
		AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
		Version assemblyVersion = assemblyName.Version;
		string versionString = "";
		if (assemblyVersion != null)
		{
			versionString = string.Format(
				" {0}.{1}.{2}",
				assemblyVersion.Major,
				assemblyVersion.Minor,
				assemblyVersion.Build
			);
		}

		this.Font = new Font("MS UI Gothic", fontSize, GraphicsUnit.Pixel);
		this.FormBorderStyle = FormBorderStyle.FixedSingle;
		this.MaximizeBox = false;
		this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		this.Text = "TrainVoiceGuide" + versionString;
		this.ClientSize = GetSizeOnGrid(25, 19);
		Font largeFont = new Font("MS UI Gothic", fontSize * 1.8f, GraphicsUnit.Pixel);
		SuspendLayout();

		distanceTimeGroup = CreateControl<GroupBox>(this, 0.5f, 0.5f, 24, 8.5f);
		distanceTimeGroup.Text = "残り距離・時間";
		distanceTimeGroup.SuspendLayout();
		distanceTimeEnableLabel = CreateControl<Label>(distanceTimeGroup, 0.5f, 1, 4, 1);
		distanceTimeEnableLabel.Text = "読み上げ";
		distanceTimeEnablePanel = CreateControl<Panel>(distanceTimeGroup, 4.5f, 1, 19, 1);
		distanceTimeEnablePanel.SuspendLayout();
		distanceTimeEnableNoneRadio = CreateControl<RadioButton>(distanceTimeEnablePanel, 0, 0, 3, 1);
		distanceTimeEnableNoneRadio.Text = "なし";
		distanceTimeEnableTimeRadio = CreateControl<RadioButton>(distanceTimeEnablePanel, 3, 0, 3, 1);
		distanceTimeEnableTimeRadio.Text = "時間";
		distanceTimeEnableDistanceRadio = CreateControl<RadioButton>(distanceTimeEnablePanel, 6, 0, 3, 1);
		distanceTimeEnableDistanceRadio.Text = "距離";
		distanceTimeEnableTimeDistanceRadio = CreateControl<RadioButton>(distanceTimeEnablePanel, 9, 0, 5, 1);
		distanceTimeEnableTimeDistanceRadio.Text = "時間+距離";
		distanceTimeEnableDistanceTimeRadio = CreateControl<RadioButton>(distanceTimeEnablePanel, 14, 0, 5, 1);
		distanceTimeEnableDistanceTimeRadio.Text = "距離+時間";
		distanceTimeEnableDistanceTimeRadio.Checked = true;
		distanceTimeEnablePanel.ResumeLayout();
		distanceTimeSpeedLabel = CreateControl<Label>(distanceTimeGroup, 0.5f, 2.5f, 4, 1);
		distanceTimeSpeedLabel.Text = "音声速度";
		distanceTimeSpeedScroll = CreateControl<HScrollBar>(distanceTimeGroup, 4.5f, 2.5f, 12.5f, 1);
		distanceTimeSpeedScroll.Minimum = 50;
		distanceTimeSpeedScroll.Maximum = 324; // なんか知らんがこれで300までになってくれた f*ck
		distanceTimeSpeedScroll.LargeChange = 25;
		distanceTimeSpeedScroll.Value = 100;
		distanceTimeSpeedScroll.Scroll += (s, e) => {
			distanceTimeSpeedInput.Value = Math.Min(distanceTimeSpeedInput.Maximum, distanceTimeSpeedScroll.Value);
		};
		distanceTimeSpeedInput = CreateControl<NumericUpDown>(distanceTimeGroup, 17.5f, 2.5f, 2.5f, 1);
		distanceTimeSpeedInput.Minimum = 50;
		distanceTimeSpeedInput.Maximum = 300;
		distanceTimeSpeedInput.Value = 100;
		distanceTimeSpeedInput.ValueChanged += (s, e) => {
			distanceTimeSpeedScroll.Value = (int)distanceTimeSpeedInput.Value;
		};
		distanceTimeSpeedTestButton = CreateControl<Button>(distanceTimeGroup, 20.5f, 2.4f, 3, 1.2f);
		distanceTimeSpeedTestButton.Text = "テスト";
		distanceTimeTriggerGroup = CreateControl<GroupBox>(distanceTimeGroup, 0.5f, 4, 23, 4);
		distanceTimeTriggerGroup.Text = "読み上げるタイミング";
		distanceTimeTriggerStartCheck = CreateControl<CheckBox>(distanceTimeTriggerGroup, 0.5f, 1, 5, 1);
		distanceTimeTriggerStartCheck.Text = "発車時";
		distanceTimeTriggerStopCheck = CreateControl<CheckBox>(distanceTimeTriggerGroup, 5.5f, 1, 5, 1);
		distanceTimeTriggerStopCheck.Text = "停車時";
		distanceTimeTriggerStopCheck.Checked = true;
		distanceTimeTriggerNextStationChangeCheck = CreateControl<CheckBox>(distanceTimeTriggerGroup, 10.5f, 1, 6, 1);
		distanceTimeTriggerNextStationChangeCheck.Text = "次駅変化時";
		distanceTimeTriggerNextStationChangeCheck.Checked = true;
		distanceTimeTriggerDistanceCheck = CreateControl<CheckBox>(distanceTimeTriggerGroup, 0.5f, 2.5f, 4, 1);
		distanceTimeTriggerDistanceCheck.Text = "残り距離";
		distanceTimeTriggerDistanceInput = CreateControl<NumericUpDown>(distanceTimeTriggerGroup, 4.5f, 2.5f, 2.5f, 1);
		distanceTimeTriggerDistanceInput.Minimum = 1;
		distanceTimeTriggerDistanceInput.Maximum = Decimal.MaxValue;
		distanceTimeTriggerDistanceInput.Value = 500;
		distanceTimeTriggerDistanceLabel = CreateControl<Label>(distanceTimeTriggerGroup, 7, 2.5f, 3, 1);
		distanceTimeTriggerDistanceLabel.Text = "m ごと";
		distanceTimeTriggerTimeCheck = CreateControl<CheckBox>(distanceTimeTriggerGroup, 10.5f, 2.5f, 4, 1);
		distanceTimeTriggerTimeCheck.Text = "残り時間";
		distanceTimeTriggerTimeCheck.Checked = true;
		distanceTimeTriggerTimeInput = CreateControl<NumericUpDown>(distanceTimeTriggerGroup, 14.5f, 2.5f, 2.5f, 1);
		distanceTimeTriggerTimeInput.Minimum = 1;
		distanceTimeTriggerTimeInput.Maximum = Decimal.MaxValue;
		distanceTimeTriggerTimeInput.Value = 60;
		distanceTimeTriggerTimeLabel = CreateControl<Label>(distanceTimeTriggerGroup, 17, 2.5f, 3, 1);
		distanceTimeTriggerTimeLabel.Text = "秒ごと";
		distanceTimeGroup.ResumeLayout();

		speedGroup = CreateControl<GroupBox>(this, 0.5f, 9.5f, 24, 4.5f);
		speedGroup.Text = "制限速度・予告";
		speedGroup.SuspendLayout();
		speedEnableLabel = CreateControl<Label>(speedGroup, 0.5f, 1, 4, 1);
		speedEnableLabel.Text = "読み上げ";
		speedEnablePanel = CreateControl<Panel>(speedGroup, 4.5f, 1, 19, 1);
		speedEnablePanel.SuspendLayout();
		speedNoneRadio = CreateControl<RadioButton>(speedEnablePanel, 0, 0, 3, 1);
		speedNoneRadio.Text = "なし";
		speedLimitRadio = CreateControl<RadioButton>(speedEnablePanel, 3, 0, 3, 1);
		speedLimitRadio.Text = "制限";
		speedNoticeRadio = CreateControl<RadioButton>(speedEnablePanel, 6, 0, 3, 1);
		speedNoticeRadio.Text = "予告";
		speedNoticeRadio.Checked = true;
		speedLimitNoticeRadio = CreateControl<RadioButton>(speedEnablePanel, 9, 0, 5, 1);
		speedLimitNoticeRadio.Text = "制限+予告";
		speedNoticeLimitRadio = CreateControl<RadioButton>(speedEnablePanel, 14, 0, 5, 1);
		speedNoticeLimitRadio.Text = "予告+制限";
		speedSpeedLabel = CreateControl<Label>(speedGroup, 0.5f, 2.5f, 4, 1);
		speedSpeedLabel.Text = "音声速度";
		speedSpeedScroll = CreateControl<HScrollBar>(speedGroup, 4.5f, 2.5f, 12.5f, 1);
		speedSpeedScroll.Minimum = 50;
		speedSpeedScroll.Maximum = 324; // なんか知らんがこれで300までになってくれた f*ck
		speedSpeedScroll.LargeChange = 25;
		speedSpeedScroll.Value = 100;
		speedSpeedScroll.Scroll += (s, e) => {
			speedSpeedInput.Value = Math.Min(speedSpeedInput.Maximum, speedSpeedScroll.Value);
		};
		speedSpeedInput = CreateControl<NumericUpDown>(speedGroup, 17.5f, 2.5f, 2.5f, 1);
		speedSpeedInput.Minimum = 50;
		speedSpeedInput.Maximum = 300;
		speedSpeedInput.Value = 100;
		speedSpeedInput.ValueChanged += (s, e) => {
			speedSpeedScroll.Value = (int)speedSpeedInput.Value;
		};
		speedSpeedTestButton = CreateControl<Button>(speedGroup, 20.5f, 2.4f, 3, 1.2f);
		speedSpeedTestButton.Text = "テスト";
		speedGroup.ResumeLayout();

		statusPanel = CreateControl<Panel>(this, 0.5f, 14, 24, 4.5f);
		statusPanel.SuspendLayout();
		statusNextStationTitleLabel = CreateControl<Label>(statusPanel, 0, 1, 2, 1);
		statusNextStationTitleLabel.Text = "次駅";
		statusNextStationTitleLabel.TextAlign = ContentAlignment.BottomLeft;
		statusNextStationLabel = CreateControl<Label>(statusPanel, 2, 0, 8, 2);
		statusNextStationLabel.Font = largeFont;
		//statusNextStationLabel.Text = "大道寺引上線";//"江ノ原信号場";
		statusNextStationLabel.TextAlign = ContentAlignment.BottomLeft;
		statusDistanceTitleLabel = CreateControl<Label>(statusPanel, 10, 1, 1.75f, 1);
		statusDistanceTitleLabel.Text = "距離";
		statusDistanceTitleLabel.TextAlign = ContentAlignment.BottomLeft;
		statusDistanceLabel = CreateControl<Label>(statusPanel, 11.75f, 0, 5.5f, 2);
		statusDistanceLabel.Font = largeFont;
		//statusDistanceLabel.Text = "-12345m";
		statusDistanceLabel.TextAlign = ContentAlignment.BottomRight;
		statusTimeTitleLabel = CreateControl<Label>(statusPanel, 17.25f, 1, 1.75f, 1);
		statusTimeTitleLabel.Text = "時間";
		statusTimeTitleLabel.TextAlign = ContentAlignment.BottomLeft;
		statusTimeLabel = CreateControl<Label>(statusPanel, 19, 0, 5, 2);
		statusTimeLabel.Font = largeFont;
		//statusTimeLabel.Text = "-9999秒";
		statusTimeLabel.TextAlign = ContentAlignment.BottomRight;
		statusSpeedTitleLabel = CreateControl<Label>(statusPanel, 0, 3, 2, 1);
		statusSpeedTitleLabel.Text = "速度";
		statusSpeedTitleLabel.TextAlign = ContentAlignment.BottomLeft;
		statusSpeedLabel = CreateControl<Label>(statusPanel, 2, 2, 7, 2);
		statusSpeedLabel.Font = largeFont;
		//statusSpeedLabel.Text = "-110.0km/h";
		statusSpeedLabel.TextAlign = ContentAlignment.BottomRight;
		statusLimitTitleLabel = CreateControl<Label>(statusPanel, 9, 3, 2, 1);
		statusLimitTitleLabel.Text = "制限";
		statusLimitTitleLabel.TextAlign = ContentAlignment.BottomLeft;
		statusLimitLabel = CreateControl<Label>(statusPanel, 11, 2, 5.5f, 2);
		statusLimitLabel.Font = largeFont;
		//statusLimitLabel.Text = "110km/h";
		statusLimitLabel.TextAlign = ContentAlignment.BottomRight;
		statusLimitNoticeTitleLabel = CreateControl<Label>(statusPanel, 16.5f, 3, 2, 1);
		statusLimitNoticeTitleLabel.Text = "予告";
		statusLimitNoticeTitleLabel.TextAlign = ContentAlignment.BottomLeft;
		statusLimitNoticeLabel = CreateControl<Label>(statusPanel, 18.5f, 2, 5.5f, 2);
		statusLimitNoticeLabel.Font = largeFont;
		//statusLimitNoticeLabel.Text = "110km/h";
		statusLimitNoticeLabel.TextAlign = ContentAlignment.BottomRight;
		statusPanel.ResumeLayout();

		ResumeLayout();

		distanceTimeSpeedTestButton.Click += DistanceTimeSpeedTestClickHandler;
		speedSpeedTestButton.Click += SpeedSpeedTestClickHandler;

		Load += LoadHandler;
		FormClosed += FormClosedHandler;
	}

	private void LoadHandler(object sender, EventArgs e)
	{
		RegistryIO regIO = RegistryIO.OpenForRead();
		if (regIO != null)
		{
			string distanceTimeType = regIO.GetStringValue(DistanceTimeTypeValueName);
			if (distanceTimeType == DistanceTimeTypeNoneData)
				distanceTimeEnableNoneRadio.Checked = true;
			else if (distanceTimeType == DistanceTimeTypeTimeData)
				distanceTimeEnableTimeRadio.Checked = true;
			else if (distanceTimeType == DistanceTimeTypeDistanceData)
				distanceTimeEnableDistanceRadio.Checked = true;
			else if (distanceTimeType == DistanceTimeTypeTimeDistanceData)
				distanceTimeEnableTimeDistanceRadio.Checked = true;
			else if (distanceTimeType == DistanceTimeTypeDistanceTimeData)
				distanceTimeEnableDistanceTimeRadio.Checked = true;

			int? distanceTimeSpeed = regIO.GetIntValue(DistanceTimeSpeedValueName);
			if (distanceTimeSpeed.HasValue)
			{
				int value = distanceTimeSpeed.Value;
				if (distanceTimeSpeedInput.Minimum <= value && value <= distanceTimeSpeedInput.Maximum)
				{
					distanceTimeSpeedInput.Value = value;
				}
			}

			int? distanceTimeTriggerStart = regIO.GetIntValue(DistainceTimeTriggerStartValueName);
			if (distanceTimeTriggerStart.HasValue)
				distanceTimeTriggerStartCheck.Checked = distanceTimeTriggerStart.Value != 0;
			int? distanceTimeTriggerStop = regIO.GetIntValue(DistainceTimeTriggerStopValueName);
			if (distanceTimeTriggerStop.HasValue)
				distanceTimeTriggerStopCheck.Checked = distanceTimeTriggerStop.Value != 0;
			int? distanceTimeTriggerNextStationChange = regIO.GetIntValue(DistainceTimeTriggerNextStationChangeValueName);
			if (distanceTimeTriggerNextStationChange.HasValue)
				distanceTimeTriggerNextStationChangeCheck.Checked = distanceTimeTriggerNextStationChange.Value != 0;
			int? distanceTimeTriggerDistance = regIO.GetIntValue(DistainceTimeTriggerDistanceValueName);
			if (distanceTimeTriggerDistance.HasValue)
				distanceTimeTriggerDistanceCheck.Checked = distanceTimeTriggerDistance.Value != 0;
			int? distanceTimeTriggerTime = regIO.GetIntValue(DistainceTimeTriggerTimeValueName);
			if (distanceTimeTriggerTime.HasValue)
				distanceTimeTriggerTimeCheck.Checked = distanceTimeTriggerTime.Value != 0;
			int? distanceTimeTriggerDistanceInterval = regIO.GetIntValue(DistainceTimeTriggerDistanceIntervalValueName);
			if (distanceTimeTriggerDistanceInterval.HasValue && distanceTimeTriggerDistanceInterval.Value >= distanceTimeTriggerDistanceInput.Minimum)
				distanceTimeTriggerDistanceInput.Value = distanceTimeTriggerDistanceInterval.Value;
			int? distanceTimeTriggerTimeInterval = regIO.GetIntValue(DistainceTimeTriggerTimeIntervalValueName);
			if (distanceTimeTriggerTimeInterval.HasValue && distanceTimeTriggerTimeInterval.Value >= distanceTimeTriggerTimeInput.Minimum)
				distanceTimeTriggerTimeInput.Value = distanceTimeTriggerTimeInterval.Value;

			string speedTimeType = regIO.GetStringValue(SpeedTypeValueName);
			if (speedTimeType == SpeedTypeNoneData)
				speedNoneRadio.Checked = true;
			else if (speedTimeType == SpeedTypeLimitData)
				speedLimitRadio.Checked = true;
			else if (speedTimeType == SpeedTypeNoticeData)
				speedNoticeRadio.Checked = true;
			else if (speedTimeType == SpeedTypeLimitNoticeData)
				speedLimitNoticeRadio.Checked = true;
			else if (speedTimeType == SpeedTypeNoticeLimitData)
				speedNoticeLimitRadio.Checked = true;

			int? speedSpeed = regIO.GetIntValue(SpeedSpeedValueName);
			if (speedSpeed.HasValue)
			{
				int value = speedSpeed.Value;
				if (speedSpeedInput.Minimum <= value && value <= speedSpeedInput.Maximum)
				{
					speedSpeedInput.Value = value;
				}
			}

			regIO.Close();
		}

		TrainCrewInput.Init();
		timer = new Timer();
		timer.Interval = 50;
		timer.Tick += TimerTickHandler;
		timer.Start();
	}

	private void FormClosedHandler(object sender, EventArgs e)
	{
		timer.Stop();
		TrainCrewInput.Dispose();
		testTalker.Dispose();
		infoTalker.Dispose();
		testTalker = null;
		infoTalker = null;

		RegistryIO regIO = RegistryIO.OpenForWrite();
		if (regIO != null)
		{
			string distanceTimeType = null;
			if (distanceTimeEnableNoneRadio.Checked) distanceTimeType = DistanceTimeTypeNoneData;
			else if (distanceTimeEnableTimeRadio.Checked) distanceTimeType = DistanceTimeTypeTimeData;
			else if (distanceTimeEnableDistanceRadio.Checked) distanceTimeType = DistanceTimeTypeDistanceData;
			else if (distanceTimeEnableTimeDistanceRadio.Checked) distanceTimeType = DistanceTimeTypeTimeDistanceData;
			else if (distanceTimeEnableDistanceTimeRadio.Checked) distanceTimeType = DistanceTimeTypeDistanceTimeData;
			if (distanceTimeType != null) regIO.SetValue(DistanceTimeTypeValueName, distanceTimeType);

			regIO.SetValue(DistanceTimeSpeedValueName, (int)distanceTimeSpeedInput.Value);

			regIO.SetValue(DistainceTimeTriggerStartValueName, distanceTimeTriggerStartCheck.Checked ? 1 : 0);
			regIO.SetValue(DistainceTimeTriggerStopValueName, distanceTimeTriggerStopCheck.Checked ? 1 : 0);
			regIO.SetValue(DistainceTimeTriggerNextStationChangeValueName, distanceTimeTriggerNextStationChangeCheck.Checked ? 1 : 0);
			regIO.SetValue(DistainceTimeTriggerDistanceValueName, distanceTimeTriggerDistanceCheck.Checked ? 1 : 0);
			regIO.SetValue(DistainceTimeTriggerTimeValueName, distanceTimeTriggerTimeCheck.Checked ? 1 : 0);
			regIO.SetValue(DistainceTimeTriggerDistanceIntervalValueName, (int)distanceTimeTriggerDistanceInput.Value);
			regIO.SetValue(DistainceTimeTriggerTimeIntervalValueName, (int)distanceTimeTriggerTimeInput.Value);

			string speedType = null;
			if (speedNoneRadio.Checked) speedType = SpeedTypeNoneData;
			else if (speedLimitRadio.Checked) speedType = SpeedTypeLimitData;
			else if (speedNoticeRadio.Checked) speedType = SpeedTypeNoticeData;
			else if (speedLimitNoticeRadio.Checked) speedType = SpeedTypeLimitNoticeData;
			else if (speedNoticeLimitRadio.Checked) speedType = SpeedTypeNoticeLimitData;
			if (speedType != null) regIO.SetValue(SpeedTypeValueName, speedType);

			regIO.SetValue(SpeedSpeedValueName, (int)speedSpeedInput.Value);

			regIO.Close();
		}
	}

	private void TimerTickHandler(object sender, EventArgs e)
	{
		GameState gameState = TrainCrewInput.gameState;
		TrainState trainState = TrainCrewInput.GetTrainState();
		if (gameState.gameScreen != GameScreen.MainGame && gameState.gameScreen != GameScreen.MainGame_Pause)
		{
			statusNextStationLabel.Text = "---";
			statusDistanceLabel.Text = "---m";
			statusTimeLabel.Text = "---秒";
			statusSpeedLabel.Text = "---.-km/h";
			statusLimitLabel.Text = "---km/h";
			statusLimitNoticeLabel.Text = "---km/h";

			prevNextStationIndex = null;
			prevNextStationDistance = null;
			prevNextStationTime = null;
			prevSpeed = null;
			prevSpeedLimit = null;
			prevSpeedLimitNotice = null;
		}
		else
		{
			string nextStationName = "---";
			int nextStationIndex = trainState.nowStaIndex;
			float nextStationDistance = trainState.nextUIDistance;
			float? nextStationTime = null;
			float speed = trainState.Speed;
			float speedLimit = trainState.speedLimit;
			float speedLimitNotice = trainState.nextSpeedLimit;
			if (nextStationIndex < trainState.stationList.Count)
			{
				Station nextStation = trainState.stationList[nextStationIndex];
				nextStationName = nextStation.Name;
				float trainPosition = nextStation.TotalLength - trainState.nextStaDistance;
				int bestIdx = 0;
				float bestDiff = float.PositiveInfinity;
				for (int i = 0; i < trainState.stationList.Count; i++)
				{
					float position = trainState.stationList[i].TotalLength - trainState.nextUIDistance;
					float diff = Math.Abs(position - trainPosition);
					if (diff < bestDiff)
					{
						bestIdx = i;
						bestDiff = diff;
					}
				}
				nextStationTime = (float)trainState.stationList[bestIdx].ArvTime.Subtract(trainState.NowTime).TotalSeconds;
			}
			else
			{
				TrainCrewInput.RequestStaData();
			}
			statusNextStationLabel.Text = nextStationName;
			statusDistanceLabel.Text = Math.Abs(nextStationDistance) <= 3
				? string.Format("{0:F0}cm", nextStationDistance * 100)
				: string.Format("{0:F0}m", nextStationDistance);
			statusTimeLabel.Text = nextStationTime.HasValue
				? string.Format("{0:F0}秒", Math.Ceiling(nextStationTime.Value))
				: "---秒";
			statusSpeedLabel.Text = string.Format("{0:F1}km/h", speed);
			statusLimitLabel.Text = string.Format("{0:F0}km/h", speedLimit);
			statusLimitNoticeLabel.Text = speedLimitNotice >= 0 ? string.Format("{0:F0}km/h", speedLimitNotice) : "なし";

			// 残り距離・時間の読み上げを行うかの判定をする
			bool readDistanceTime = false;
			bool readDistanceTimeBecauseStop = false;
			if (prevSpeed.HasValue)
			{
				// 発車時 (速度ゼロ → 速度非ゼロ)
				if (distanceTimeTriggerStartCheck.Checked)
				{
					readDistanceTime = readDistanceTime || (prevSpeed.Value == 0 && speed != 0);
				}
				// 停車時 (速度非ゼロ → 速度ゼロ)
				if (distanceTimeTriggerStopCheck.Checked)
				{
					readDistanceTimeBecauseStop = prevSpeed.Value != 0 && speed == 0;
					readDistanceTime = readDistanceTime || readDistanceTimeBecauseStop;
				}
			}
			// 次駅変化
			if (distanceTimeTriggerNextStationChangeCheck.Checked && prevNextStationIndex.HasValue)
			{
				readDistanceTime = readDistanceTime || prevNextStationIndex.Value != nextStationIndex;
			}
			// 残り距離
			if (distanceTimeTriggerDistanceCheck.Checked && prevNextStationDistance.HasValue)
			{
				// 表示上設定した間隔の倍数になるときに読み上げたい
				float grid = (float)distanceTimeTriggerDistanceInput.Value;
				if (speed >= 0)
				{
					// 前進 たとえば500m間隔のとき、501mと500mが違うグループになるようにする
					double prevGroup = Math.Floor((Math.Round(prevNextStationDistance.Value) - 1) / grid);
					double group = Math.Floor((Math.Round(nextStationDistance) - 1) / grid);
					readDistanceTime = readDistanceTime || group < prevGroup;
				}
				else
				{
					// 後退 たとえば500m間隔のとき、499mと500mが違うグループになるようにする
					double prevGroup = Math.Floor(Math.Round(prevNextStationDistance.Value) / grid);
					double group = Math.Floor(Math.Round(nextStationDistance) / grid);
					readDistanceTime = readDistanceTime || prevGroup < group;
				}
			}
			// 残り時間
			if (distanceTimeTriggerTimeCheck.Checked && prevNextStationTime.HasValue)
			{
				// 表示上設定した間隔の倍数になるときに読み上げたい
				float grid = (float)distanceTimeTriggerTimeInput.Value;
				double prevGroup = Math.Floor((Math.Ceiling(prevNextStationTime.Value) - 1) / grid);
				double group = Math.Floor((Math.Ceiling(nextStationTime.Value) - 1) / grid);
				readDistanceTime = readDistanceTime || group < prevGroup;
			}
			// 判定の結果、読み上げるべき場合は読み上げる (ただし、既に読み上げ中のときは無視)
			if (readDistanceTime)
			{
				string time = TimeToVoiceString(nextStationTime.Value);
				string distance = DistanceToVoiceString(nextStationDistance, readDistanceTimeBecauseStop);
				string voiceStr = null;
				if (distanceTimeEnableTimeRadio.Checked) voiceStr = time;
				if (distanceTimeEnableDistanceRadio.Checked) voiceStr = distance;
				else if (distanceTimeEnableTimeDistanceRadio.Checked) voiceStr = time + "," + distance;
				else if (distanceTimeEnableDistanceTimeRadio.Checked) voiceStr = distance + "," + time;
				if (voiceStr != null)
					AddVoiceToQueue(voiceStr, (int)distanceTimeSpeedInput.Value, !readDistanceTimeBecauseStop);
			}

			// 速度制限およびその予告の読み上げを行うかの判定をする
			bool readSpeedLimit = false, readSpeedNotice = false;
			if (prevSpeedLimit.HasValue)
			{
				// 速度制限が変化した
				readSpeedLimit = readSpeedLimit || prevSpeedLimit.Value != speedLimit;
			}
			else
			{
				// 速度制限の情報が無い (ゲーム開始前) → ある
				readSpeedLimit = true; // readSpeedLimit || true
			}
			if (prevSpeedLimitNotice.HasValue)
			{
				// 速度制限予告が変化した
				// 「制限予告が制限になり、予告が無くなった」パターンは読み上げない
				readSpeedNotice = readSpeedNotice || (
					prevSpeedLimitNotice.Value != speedLimitNotice &&
					!(speedLimitNotice < 0 && prevSpeedLimitNotice.Value == speedLimit)
				);
			}
			else
			{
				// 速度制限予告の情報が無い (ゲーム開始前) → ある
				// 予告がある場合のみ、読み上げる
				readSpeedNotice = readSpeedNotice || speedLimitNotice >= 0;
			}
			// 判定の結果、読み上げるべき場合は読み上げる
			if (readSpeedLimit || readSpeedNotice)
			{
				string limit = LimitToVoiceString(speedLimit);
				string notice = NoticeToVoiceString(speedLimitNotice);
				string voiceStr = null;
				if (speedLimitRadio.Checked)
				{
					if (readSpeedLimit) voiceStr = limit;
				}
				else if (speedNoticeRadio.Checked)
				{
					if (readSpeedNotice) voiceStr = notice;
				}
				else if (speedLimitNoticeRadio.Checked)
				{
					if (readSpeedLimit && readSpeedNotice) voiceStr = limit + "," + notice;
					else if (readSpeedLimit) voiceStr = limit;
					else if (readSpeedNotice) voiceStr = notice;
				}
				else if (speedNoticeLimitRadio.Checked)
				{
					if (readSpeedLimit && readSpeedNotice) voiceStr = notice + "," + limit;
					else if (readSpeedLimit) voiceStr = limit;
					else if (readSpeedNotice) voiceStr = notice;
				}
				if (voiceStr != null)
					AddVoiceToQueue(voiceStr, (int)speedSpeedInput.Value, false);
			}

			prevNextStationIndex = nextStationIndex;
			prevNextStationDistance = nextStationDistance;
			prevNextStationTime = nextStationTime;
			prevSpeed = speed;
			prevSpeedLimit = speedLimit;
			prevSpeedLimitNotice = speedLimitNotice;
		}
	}

	private void DistanceTimeSpeedTestClickHandler(object sender, EventArgs e)
	{
		if (testTalker == null) return;
		string time = TimeToVoiceString(60);
		string distance = DistanceToVoiceString(1000);
		string voiceStr;
		if (distanceTimeEnableDistanceRadio.Checked) voiceStr = distance;
		else if (distanceTimeEnableTimeDistanceRadio.Checked) voiceStr = time + "," + distance;
		else if (distanceTimeEnableDistanceTimeRadio.Checked) voiceStr = distance + "," + time;
		else voiceStr = time; // 「なし」の場合も、テスト用に再生する
		testTalker.Play(voiceStr, (int)distanceTimeSpeedInput.Value, Talker.PlayType.OverrideIfPlaying);
	}

	private void SpeedSpeedTestClickHandler(object sender, EventArgs e)
	{
		if (testTalker == null) return;
		string limit = LimitToVoiceString(40);
		string notice = NoticeToVoiceString(25);
		string voiceStr;
		if (speedNoticeRadio.Checked) voiceStr = notice;
		else if (speedLimitNoticeRadio.Checked) voiceStr = limit + "," + notice;
		else if (speedNoticeLimitRadio.Checked) voiceStr = notice + "," + limit;
		else voiceStr = limit; // 「なし」の場合も、テスト用に再生する
		testTalker.Play(voiceStr, (int)speedSpeedInput.Value, Talker.PlayType.OverrideIfPlaying);
	}
}
