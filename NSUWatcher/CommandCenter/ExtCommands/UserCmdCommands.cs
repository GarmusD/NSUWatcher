using NSU.Shared;
using NSU.Shared.DTO.ExtCommandContent;
using NSU.Shared.Serializer;
using NSUWatcher.Interfaces;
using NSUWatcher.Interfaces.ExtCommands;

namespace NSUWatcher.CommandCenter.ExtCommands
{
    public class UserCmdCommands : IUserCmdCommands
    {
        private readonly INsuSerializer _serializer;

        public UserCmdCommands(INsuSerializer serializer)
        {
            _serializer = serializer;
        }

        public IExternalCommand ExecUserCommand(string command)
        {
            return new ExternalCommand()
            {
                Target = JKeys.UserCmd.TargetName,
                Action = JKeys.UserCmd.ActionExec,
                Content = _serializer.Serialize( new UserCmdExecCommandContent(command) )
            };
        }
    }
}
