using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{

    public class CommandApp
    {
        public Queue<ICommand> CommandQueue { get; protected set; }
        public ICommand RunningCommand { get; protected set; }

        public CommandApp()
        {
            CommandQueue = new Queue<ICommand>();
        }

        /// <summary>
        /// This adds the command to the list.
        /// </summary>
        public void AddCommand(ICommand command)
        {
            CommandQueue.Enqueue(command);
        }

        /// <summary>
        /// This is execute the first command
        /// </summary>
        public ICommand ExecuteCommand()
        {
            if (CommandQueue.Count == 0) return null;
            if (RunningCommand != null)
            {
                // Debug.Log($"Running Command {_runningCommand}");
                return null;
            }

            RunningCommand = CommandQueue.Dequeue();
            Debug.Log($"Executing Command {RunningCommand.GetType()}");
            RunningCommand.Execute(OnCommandFinished);
            return RunningCommand;
        }

        void OnCommandFinished(ICommand runningCommand)
        {
            if (runningCommand != RunningCommand) Debug.LogError("???");

            RunningCommand.MarkAsComplete();
            RunningCommand = null;
        }

    }

}