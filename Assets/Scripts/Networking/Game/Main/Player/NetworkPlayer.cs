using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayer : MonoBehaviour
{
    private Queue<PlayerCommand> commands;

    void Update()
    {
        while (commands?.Count > 0)
        {
            PlayerCommand cmd = commands.Dequeue();
            ProcessCommand(cmd);
        }
    }

    private void ProcessCommand(PlayerCommand cmd)
    {
        if (cmd.Type == PlayerCommandType.Move)
        {
            ProcessPlayerMoveCommand(cmd);
        }
    }

    private void ProcessPlayerMoveCommand(PlayerCommand cmd)
    {
        Debug.LogFormat($" (Server) Moving Player. New Position = X: {0}, Y: {1}, Z: {2}", cmd.endingPosition.x, cmd.endingPosition.y, cmd.endingPosition.z);
        Debug.LogFormat($" (Server) Rotating Player. New Rotation = X: {0}, Y: {1}, Z: {2}, W: {3}", cmd.endingRotation.x, cmd.endingRotation.y, cmd.endingRotation.z, cmd.endingRotation.w);
        transform.localPosition = Vector3.Lerp(cmd.startingPosition, cmd.endingPosition, 100 * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(cmd.startingRotation, cmd.endingRotation, 100 * Time.deltaTime);
    }

    public void QueueCommand(PlayerCommand cmd)
    {
        if (commands is null)
        {
            commands = new Queue<PlayerCommand>();
        }

        commands.Enqueue(cmd);
    }

    public PlayerCommand GetCurrentSnapshot(int playerId)
    {
        PlayerCommand cmd = new PlayerCommand()
                                .OfType(PlayerCommandType.Snapshot)
                                .WithPlayerId(playerId);

        cmd.currentPosition = transform.localPosition;
        cmd.currentRotation = transform.localRotation;

        return cmd;
    }

}
