using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RICADO.Omron
{
    public enum enPLCType
    {
        NJ_NX_Series,
        C_Series,
        Unknown,
    }

    public enum enMemoryBitDataType
    {
        DataMemory = 0x2,
        CommonIO = 0x30,
        Work = 0x31,
        Holding = 0x32,
        Auxiliary = 0x33,
    }

    public enum enMemoryWordDataType
    {
        DataMemory = 0x82,
        CommonIO = 0xB0,
        Work = 0xB1,
        Holding = 0xB2,
        Auxiliary = 0xB3,
    }

    public enum enFunctionCode
    {
        MemoryArea = 0x01,
        ParameterArea = 0x02,
        ProgramArea = 0x03,
        OperatingMode = 0x04,
        MachineConfiguration = 0x05,
        Status = 0x06,
        TimeData = 0x07,
        MessageDisplay = 0x09,
        AccessRights = 0x0C,
        ErrorLog = 0x21,
        FINSWriteLog = 0x21,
        FileMemory = 0x22,
        Debugging = 0x23,
        SerialGateway = 0x28,
    }

    public enum enMemoryAreaFunctionCode
    {
        Read = 0x01,
        Write = 0x02,
        Fill = 0x03,
        MultipleRead = 0x04,
        Transfer = 0x05,
    }

    public enum enParameterAreaFunctionCode
    {
        Read = 0x01,
        Write = 0x02,
        Fill = 0x03,
    }

    public enum enProgramAreaFunctionCode
    {
        Read = 0x06,
        Write = 0x07,
        Clear = 0x08,
    }

    public enum enOperatingModeFunctionCode
    {
        RunMode = 0x01,
        StopMode = 0x02,
    }

    public enum enMachineConfigurationFunctionCode
    {
        ReadCPUUnitData = 0x01,
        ReadConnectionData = 0x02,
    }

    public enum enStatusFunctionCode
    {
        ReadCPUUnitStatus = 0x01,
        ReadCycleTime = 0x20,
    }

    public enum enTimeDataFunctionCode
    {
        ReadClock = 0x01,
        WriteClock = 0x02,
    }

    public enum enMessageDisplayFunctionCode
    {
        Read = 0x20,
    }

    public enum enAccessRightsFunctionCode
    {
        Acquire = 0x01,
        ForcedAcquire = 0x02,
        Release = 0x03,
    }

    public enum enErrorLogFunctionCode
    {
        ClearMessages = 0x01,
        Read = 0x02,
        ClearLog = 0x03,
    }

    public enum enFinsWriteLogFunctionCode
    {
        Read = 0x40,
        Clear = 0x41,
    }

    public enum enFileMemoryFunctionCode
    {
        ReadFileName = 0x01,
        ReadSingleFile = 0x02,
        WriteSingleFile = 0x03,
        FormatMemory = 0x04,
        DeleteFile = 0x05,
        CopyFile = 0x07,
        ChangeFileName = 0x08,
        MemoryAreaTransfer = 0x0A,
        ParameterAreaTransfer = 0x0B,
        ProgramAreaTransfer = 0x0C,
        CreateOrDeleteDirectory = 0x15,
    }

    public enum enDebuggingFunctionCode
    {
        ForceBits = 0x01,
        ClearForcedBits = 0x02,
    }

    public enum enSerialGatewayFunctionCode
    {
        ConvertToCompoWayFCommand = 0x03,
        ConvertToModbusRTUCommand = 0x04,
        ConvertToModbusASCIICommand = 0x05,
    }
}
