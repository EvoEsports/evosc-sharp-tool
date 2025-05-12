using ILRepacking;

namespace EvoSC.Tool.Utils;

public class ILRepackUtils
{
    public static void PackDependencies()
    {
        var ilRepack = new ILRepack(new RepackOptions
        {
            ExcludeFile = null,
            AllowDuplicateResources = false,
            AllowMultipleAssemblyLevelAttributes = false,
            AllowWildCards = false,
            AllowZeroPeKind = false,
            AttributeFile = null,
            Closed = false,
            CopyAttributes = false,
            DebugInfo = false,
            DelaySign = false,
            FileAlignment = 0,
            InputAssemblies = new string[]
            {
            },
            Internalize = false,
            ExcludeInternalizeSerializable = false,
            KeyFile = null,
            KeyContainer = null,
            Parallel = false,
            PauseBeforeExit = false,
            Log = false,
            LogFile = null,
            OutputFile = null,
            PublicKeyTokens = false,
            StrongNameLost = false,
            TargetKind = null,
            TargetPlatformDirectory = null,
            TargetPlatformVersion = null,
            SearchDirectories = null,
            UnionMerge = false,
            Version = null,
            PreserveTimestamp = false,
            SkipConfigMerge = false,
            MergeIlLinkerFiles = false,
            XmlDocumentation = false,
            LogVerbose = false,
            NoRepackRes = false,
            KeepOtherVersionReferences = false,
            LineIndexation = false,
            InternalizeAssemblies = null,
            AllowAllDuplicateTypes = false,
            RepackDropAttribute = null,
            RenameInternalized = false
        });
    }
}