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

        private byte _functionCode;
        private byte _subFunctionCode;
        private byte _mainResponseCode;
        private byte _subResponseCode;

        private byte[] _data;

        #endregion


        #region Public Properties

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
                return _responseData;
            }
            private set
            {
                _responseData = value;
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

            response.MainResponseCode = responseCode[0];

            response.SubResponseCode = responseCode[1];

            if (response.MainResponseCode != 0 || response.SubResponseCode != 0)
            {
                if (response.MainResponseCode == 0 && response.SubResponseCode == 64)
                {
                    // Ignore Non-Fatal CPU Unit Errors
                    // NOTE: Bit #6 being on means that a Non-Fatal CPU Unit Error has occurred (e.g. Battery Error)
                }
                else
                {
                    throw new FINSException("Received Main Response Code '" + response.MainResponseCode + "' and Sub Response Code '" + response.SubResponseCode + "' from the PLC");
                }
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

        #endregion
    }
}
