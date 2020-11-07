using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public List<Player> players = new List<Player>();

    public void AddPlayer(uint id)
    {

    }

    public void RemovePlayer(uint id)
    {

    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    public int GetPlayingPlayerCount()
    {
        // If waiting they have no entities to control
        // If playing they have entities to control
        int count = 0;

        foreach(Player p in players)
        {
            if(p.controlledEntities.Count > 0)
            {
                count++;
            }
        }

        return count;
    }
}

[Serializable]
public class Player {

    // ID is player's connection id
    public uint id;
    public string username;

    public List<int> controlledEntities;

    public bool IsPlaying()
    {
        return controlledEntities.Count > 0;
    }

    public bool IsAlive()
    {
        return false;
    }
}
