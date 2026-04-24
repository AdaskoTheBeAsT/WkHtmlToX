using System;
using System.IO;
using AdaskoTheBeAsT.WkHtmlToX.Utils;

namespace AdaskoTheBeAsT.WkHtmlToX.Abstractions;

internal interface IWkHtmlToXModule
{
    int Initialize(int useGraphics);

    int Terminate();

    int ExtendedQt();

    string GetLibraryVersion();

    IntPtr CreateGlobalSettings();

    void DestroyGlobalSetting(IntPtr settings);

    int SetGlobalSetting(
        IntPtr settings,
        string name,
        string? value);

    string GetGlobalSetting(
        IntPtr settings,
        string name);

    IntPtr CreateConverter(IntPtr globalSettings);

    void DestroyConverter(IntPtr converter);

    void SetWarningCallback(IntPtr converter, StringCallback callback);

    void SetErrorCallback(IntPtr converter, StringCallback callback);

    void SetPhaseChangedCallback(IntPtr converter, VoidCallback callback);

    void SetProgressChangedCallback(IntPtr converter, IntCallback callback);

    void SetFinishedCallback(IntPtr converter, IntCallback callback);

    bool Convert(IntPtr converter);

    int GetCurrentPhase(IntPtr converter);

    string GetPhaseDescription(IntPtr converter, int phase);

    string GetProgressDescription(IntPtr converter);

    int GetPhaseCount(IntPtr converter);

    int GetHttpErrorCode(IntPtr converter);

    void GetOutput(IntPtr converter, Stream stream);

    void GetOutput(IntPtr converter, Func<int, Stream> createStreamFunc);
}
