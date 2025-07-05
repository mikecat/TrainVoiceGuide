TARGET=TrainVoiceGuide.exe
OPTIONS= \
	/target:winexe \
	/platform:x86 \
	/optimize+ \
	/warn:4 \
	/codepage:65001 \
	/reference:TrainCrewInput.dll

SOURCES= \
	AssemblyInfo.cs \
	TrainVoiceGuide.cs \
	RegistryIO.cs \
	Talker.cs

$(TARGET): $(SOURCES)
	csc /out:$@ $(OPTIONS) $^
