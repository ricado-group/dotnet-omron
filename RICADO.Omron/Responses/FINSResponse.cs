using System;
using RICADO.Omron.Requests;

namespace RICADO.Omron.Responses
{
    internal class FINSResponse
    {
        #region Constants

        internal const int HEADER_LENGTH = 10;
        internal const int COMMAND_LENGTH = 2;
        internal const int RESPONSE_CODE_LENGTH = 2;

        #endregion


        #region Private Properties

        private byte _serviceId;

        private byte _functionCode;
        private byte _subFunctionCode;
        private byte _mainResponseCode;
        private byte _subResponseCode;

        private byte[] _data;

        #endregion


        #region Internal Properties

        internal byte ServiceID
        {
            get
            {
                return _serviceId;
            }
            set
            {
                _serviceId = value;
            }
        }

        internal byte FunctionCode
        {
            get
            {
                return _functionCode;
            }
            private set
            {
                _functionCode = value;
            }
        }

        internal byte SubFunctionCode
        {
            get
            {
                return _subFunctionCode;
            }
            set
            {
                _subFunctionCode = value;
            }
        }

        internal byte MainResponseCode
        {
            get
            {
                return _mainResponseCode;
            }
            set
            {
                _mainResponseCode = value;
            }
        }

        internal byte SubResponseCode
        {
            get
            {
                return _subResponseCode;
            }
            set
            {
                _subResponseCode = value;
            }
        }

        internal byte[] Data
        {
            get
            {
                return _data;
            }
            private set
            {
                _data = value;
            }
        }

        #endregion


        #region Constructors

        private FINSResponse()
        {
        }

        #endregion


        #region Internal Methods

        internal static FINSResponse CreateNew(Memory<byte> message, FINSRequest request)
        {
            if(message.Length < HEADER_LENGTH + COMMAND_LENGTH + RESPONSE_CODE_LENGTH)
            {
                throw new FINSException("The FINS Response Message Length was too short");
            }

            FINSResponse response = new FINSResponse();

            byte[] header = message.Slice(0, HEADER_LENGTH).ToArray();

            response.ServiceID = header[9];

            byte[] command = message.Slice(HEADER_LENGTH, COMMAND_LENGTH).ToArray();

            if(ValidateFunctionCode(command[0]) == false)
            {
                throw new FINSException("Invalid Function Code '" + command[0].ToString() + "'");
            }

            response.FunctionCode = command[0];

            if (response.FunctionCode != request.FunctionCode)
            {
                throw new FINSException("Unexpected Function Code '" + Enum.GetName(typeof(enFunctionCode), response.FunctionCode) + "' - Expecting '" + Enum.GetName(typeof(enFunctionCode), request.FunctionCode) + "'");
            }

            if(ValidateSubFunctionCode(command[0], command[1]) == false)
            {
                throw new FINSException("Invalid Sub Function Code '" + command[1].ToString() + "' for Function Code '" + command[0].ToString() + "'");
            }

            response.SubFunctionCode = command[1];

            if (response.SubFunctionCode != request.SubFunctionCode)
            {
                throw new FINSException("Unexpected Sub Function Code '" + getSubFunctionCodeName(response.FunctionCode, response.SubFunctionCode) + "' - Expecting '" + getSubFunctionCodeName(request.FunctionCode, request.SubFunctionCode) + "'");
            }

            byte[] responseCode = message.Slice(HEADER_LENGTH + COMMAND_LENGTH, RESPONSE_CODE_LENGTH).ToArray();

            if(hasNetworkRelayError(responseCode[0]))
            {
                throw new FINSException("A Network Relay Error has occurred");
            }

            response.MainResponseCode = getMainResponseCode(responseCode[0]);

            response.SubResponseCode = getSubResponseCode(responseCode[1]);

            throwIfResponseError(response.MainResponseCode, response.SubResponseCode);

            if(request.ServiceID != response.ServiceID)
            {
                throw new FINSException("The Service ID for the FINS Request '" + request.ServiceID + "' did not match the FINS Response '" + response.ServiceID + "'");
            }

            response.Data = message.Length > HEADER_LENGTH + COMMAND_LENGTH + RESPONSE_CODE_LENGTH ? message.Slice(HEADER_LENGTH + COMMAND_LENGTH + RESPONSE_CODE_LENGTH, message.Length - (HEADER_LENGTH + COMMAND_LENGTH + RESPONSE_CODE_LENGTH)).ToArray() : new byte[0];

            return response;
        }

        internal static bool ValidateFunctionCode(byte functionCode)
        {
            return Enum.IsDefined(typeof(enFunctionCode), functionCode);
        }

        internal static bool ValidateSubFunctionCode(byte functionCode, byte subFunctionCode)
        {
            switch((enFunctionCode)functionCode)
            {
                case enFunctionCode.AccessRights:
                    return Enum.IsDefined(typeof(enAccessRightsFunctionCode), subFunctionCode);

                case enFunctionCode.Debugging:
                    return Enum.IsDefined(typeof(enDebuggingFunctionCode), subFunctionCode);

                case enFunctionCode.ErrorLog:
                    return Enum.IsDefined(typeof(enErrorLogFunctionCode), subFunctionCode) || Enum.IsDefined(typeof(enFinsWriteLogFunctionCode), subFunctionCode);

                case enFunctionCode.FileMemory:
                    return Enum.IsDefined(typeof(enFileMemoryFunctionCode), subFunctionCode);

                case enFunctionCode.MachineConfiguration:
                    return Enum.IsDefined(typeof(enMachineConfigurationFunctionCode), subFunctionCode);

                case enFunctionCode.MemoryArea:
                    return Enum.IsDefined(typeof(enMemoryAreaFunctionCode), subFunctionCode);

                case enFunctionCode.MessageDisplay:
                    return Enum.IsDefined(typeof(enMessageDisplayFunctionCode), subFunctionCode);

                case enFunctionCode.OperatingMode:
                    return Enum.IsDefined(typeof(enOperatingModeFunctionCode), subFunctionCode);

                case enFunctionCode.ParameterArea:
                    return Enum.IsDefined(typeof(enParameterAreaFunctionCode), subFunctionCode);

                case enFunctionCode.ProgramArea:
                    return Enum.IsDefined(typeof(enProgramAreaFunctionCode), subFunctionCode);

                case enFunctionCode.SerialGateway:
                    return Enum.IsDefined(typeof(enSerialGatewayFunctionCode), subFunctionCode);

                case enFunctionCode.Status:
                    return Enum.IsDefined(typeof(enStatusFunctionCode), subFunctionCode);

                case enFunctionCode.TimeData:
                    return Enum.IsDefined(typeof(enTimeDataFunctionCode), subFunctionCode);
            }

            return false;
        }

        #endregion


        #region Private Methods

        private static string getSubFunctionCodeName(byte functionCode, byte subFunctionCode)
        {
            switch ((enFunctionCode)functionCode)
            {
                case enFunctionCode.AccessRights:
                    return Enum.GetName(typeof(enAccessRightsFunctionCode), subFunctionCode);

                case enFunctionCode.Debugging:
                    return Enum.GetName(typeof(enDebuggingFunctionCode), subFunctionCode);

                case enFunctionCode.ErrorLog:
                    return Enum.IsDefined(typeof(enErrorLogFunctionCode), subFunctionCode) ? Enum.GetName(typeof(enErrorLogFunctionCode), subFunctionCode) : Enum.GetName(typeof(enFinsWriteLogFunctionCode), subFunctionCode);

                case enFunctionCode.FileMemory:
                    return Enum.GetName(typeof(enFileMemoryFunctionCode), subFunctionCode);

                case enFunctionCode.MachineConfiguration:
                    return Enum.GetName(typeof(enMachineConfigurationFunctionCode), subFunctionCode);

                case enFunctionCode.MemoryArea:
                    return Enum.GetName(typeof(enMemoryAreaFunctionCode), subFunctionCode);

                case enFunctionCode.MessageDisplay:
                    return Enum.GetName(typeof(enMessageDisplayFunctionCode), subFunctionCode);

                case enFunctionCode.OperatingMode:
                    return Enum.GetName(typeof(enOperatingModeFunctionCode), subFunctionCode);

                case enFunctionCode.ParameterArea:
                    return Enum.GetName(typeof(enParameterAreaFunctionCode), subFunctionCode);

                case enFunctionCode.ProgramArea:
                    return Enum.GetName(typeof(enProgramAreaFunctionCode), subFunctionCode);

                case enFunctionCode.SerialGateway:
                    return Enum.GetName(typeof(enSerialGatewayFunctionCode), subFunctionCode);

                case enFunctionCode.Status:
                    return Enum.GetName(typeof(enStatusFunctionCode), subFunctionCode);

                case enFunctionCode.TimeData:
                    return Enum.GetName(typeof(enTimeDataFunctionCode), subFunctionCode);
            }

            return "Unknown";
        }

        private static bool hasNetworkRelayError(byte responseCode)
        {
            return (responseCode & (1 << 7)) != 0;
        }

        private static byte getMainResponseCode(byte value)
        {
            byte ignoredBits = 0x80;

            return (byte)(value & (byte)~ignoredBits);
        }

        private static byte getSubResponseCode(byte value)
        {
            byte ignoredBits = 0xC0;

            return (byte)(value & (byte)~ignoredBits);
        }

        private static void throwIfResponseError(byte mainCode, byte subCode)
        {
            if(mainCode == 0 && subCode == 0)
            {
                return;
            }
            
            FINSException exception = mainCode switch
            {
                0x00 => subCode switch
                {
                    0x01 => new FINSException("Normal Completion (0x00) - Service was Canceled (0x01)"),
                    _ => null,
                },
                0x01 => subCode switch
                {
                    0x01 => new FINSException("Local Node Error (0x01) - The Local Node was not found within the Network (0x01)"),
                    _ => new FINSException("Local Node Error (0x01) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x02 => subCode switch
                {
                    0x01 => new FINSException("Destination Node Error (0x02) - The Destination Node was not found within the Network (0x01)"),
                    0x02 => new FINSException("Destination Node Error (0x02) - The Destination Unit could not be found (0x02)"),
                    0x04 => new FINSException("Destination Node Error (0x02) - The Destination Node was Busy (0x04)"),
                    0x05 => new FINSException("Destination Node Error (0x02) - Response Timeout (0x05)"),
                    _ => new FINSException("Destination Node Error (0x02) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x03 => subCode switch
                {
                    0x01 => new FINSException("Controller Error (0x03) - Communications Controller Error (0x01)"),
                    0x02 => new FINSException("Controller Error (0x03) - CPU Unit Error (0x02)"),
                    0x03 => new FINSException("Controller Error (0x03) - Controller Board Error (0x03)"),
                    0x04 => new FINSException("Controller Error (0x03) - Unit Number Error (0x04)"),
                    _ => new FINSException("Controller Error (0x03) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x04 => subCode switch
                {
                    0x01 => new FINSException("Service Unsupported Error (0x04) - Undefined Command (0x01)"),
                    0x02 => new FINSException("Service Unsupported Error (0x04) - Command Not Supported by Model/Version (0x02)"),
                    _ => new FINSException("Service Unsupported Error (0x04) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x05 => subCode switch
                {
                    0x01 => new FINSException("Routing Table Error (0x05) - Destination Address Setting Error (0x01)"),
                    0x02 => new FINSException("Routing Table Error (0x05) - No Routing Tables (0x02)"),
                    0x03 => new FINSException("Routing Table Error (0x05) - Routing Table Error (0x03)"),
                    0x04 => new FINSException("Routing Table Error (0x05) - Too Many Relays (0x04)"),
                    _ => new FINSException("Routing Table Error (0x05) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x10 => subCode switch
                {
                    0x01 => new FINSException("Command Format Error (0x10) - Command Data is too Long (0x01)"),
                    0x02 => new FINSException("Command Format Error (0x10) - Command Data is too Short (0x02)"),
                    0x03 => new FINSException("Command Format Error (0x10) - Elements Length and Values Length do not Match (0x03)"),
                    0x04 => new FINSException("Command Format Error (0x10) - Command Format Error (0x04)"),
                    0x05 => new FINSException("Command Format Error (0x10) - Header Error (0x05)"),
                    _ => new FINSException("Command Format Error (0x10) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x11 => subCode switch
                {
                    0x01 => new FINSException("Parameter Error (0x11) - No Memory Area Specified (0x01)"),
                    0x02 => new FINSException("Parameter Error (0x11) - Access Size Error (0x02)"),
                    0x03 => new FINSException("Parameter Error (0x11) - Address Range Error (0x03)"),
                    0x04 => new FINSException("Parameter Error (0x11) - Address Range Exceeded (0x04)"),
                    0x06 => new FINSException("Parameter Error (0x11) - Program Missing (0x06)"),
                    0x09 => new FINSException("Parameter Error (0x11) - Relational Error (0x09)"),
                    0x0A => new FINSException("Parameter Error (0x11) - Duplicate Data Access (0x0A)"),
                    0x0B => new FINSException("Parameter Error (0x11) - Response Data is too Long (0x0B)"),
                    0x0C => new FINSException("Parameter Error (0x11) - Parameter Error (0x0C)"),
                    _ => new FINSException("Parameter Error (0x11) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x20 => subCode switch
                {
                    0x02 => new FINSException("Read not Possible Error (0x20) - The Program Area is Protected (0x02)"),
                    0x03 => new FINSException("Read not Possible Error (0x20) - Table Missing (0x03)"),
                    0x04 => new FINSException("Read not Possible Error (0x20) - Data Missing (0x04)"),
                    0x05 => new FINSException("Read not Possible Error (0x20) - Program Missing (0x05)"),
                    0x06 => new FINSException("Read not Possible Error (0x20) - File Missing (0x06)"),
                    0x07 => new FINSException("Read not Possible Error (0x20) - Data Mismatch (0x07)"),
                    _ => new FINSException("Read not Possible Error (0x20) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x21 => subCode switch
                {
                    0x01 => new FINSException("Write not Possible Error (0x21) - The Specified Area is Read-Only (0x01)"),
                    0x02 => new FINSException("Write not Possible Error (0x21) - The Program Area is Protected (0x02)"),
                    0x03 => new FINSException("Write not Possible Error (0x21) - Cannot Register (0x03)"),
                    0x05 => new FINSException("Write not Possible Error (0x21) - Program Missing (0x05)"),
                    0x06 => new FINSException("Write not Possible Error (0x21) - File Missing (0x06)"),
                    0x07 => new FINSException("Write not Possible Error (0x21) - File Name already Exists (0x07)"),
                    0x08 => new FINSException("Write not Possible Error (0x21) - Cannot Change (0x08)"),
                    _ => new FINSException("Write not Possible Error (0x21) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x22 => subCode switch
                {
                    0x01 => new FINSException("Not Executable in Current Mode (0x22) - Not Possible during Execution (0x01)"),
                    0x02 => new FINSException("Not Executable in Current Mode (0x22) - Not Possible while Running (0x02)"),
                    0x03 => new FINSException("Not Executable in Current Mode (0x22) - PLC is in Program Mode (0x03)"),
                    0x04 => new FINSException("Not Executable in Current Mode (0x22) - PLC is in Debug Mode (0x04)"),
                    0x05 => new FINSException("Not Executable in Current Mode (0x22) - PLC is in Monitor Mode (0x05)"),
                    0x06 => new FINSException("Not Executable in Current Mode (0x22) - PLC is in Run Mode (0x06)"),
                    0x07 => new FINSException("Not Executable in Current Mode (0x22) - Specified Node is not a Polling Node (0x07)"),
                    0x08 => new FINSException("Not Executable in Current Mode (0x22) - Step Cannot be Executed (0x08)"),
                    _ => new FINSException("Not Executable in Current Mode (0x22) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x23 => subCode switch
                {
                    0x01 => new FINSException("No Such Device (0x23) - File Device Missing (0x01)"),
                    0x02 => new FINSException("No Such Device (0x23) - Memory Missing (0x02)"),
                    0x03 => new FINSException("No Such Device (0x23) - Clock Missing (0x03)"),
                    _ => new FINSException("No Such Device (0x23) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                0x24 => subCode switch
                {
                    0x01 => new FINSException("Cannot Start/Stop (0x24) - Table Missing (0x01)"),
                    _ => new FINSException("Cannot Start/Stop (0x24) - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
                },
                _ => new FINSException("Unknown Error - Main Response Code (0x" + mainCode.ToString("X2") + ") - Sub Response Code (0x" + subCode.ToString("X2") + ")"),
            };

            if(exception != null)
            {
                throw exception;
            }
        }

        #endregion
    }
}
