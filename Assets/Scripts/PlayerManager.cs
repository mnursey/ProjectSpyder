using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public List<Player> players = new List<Player>();

    public Player GetPlayer(uint id)
    {
        return players.Find(x => x.id == id);
    }

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

    public List<PlayerData> GetData()
    {
        List<PlayerData> data = new List<PlayerData>();

        foreach(Player p in players)
        {
            data.Add(p.GetData());
        }

        return data;
    }

    public void SetData(List<PlayerData> pd)
    {
        players = new List<Player>();

        foreach(PlayerData d in pd)
        {
            players.Add(new Player(d));
        }
    }

    public List<PlayerState> GetState()
    {
        List<PlayerState> state = new List<PlayerState>();

        foreach (Player p in players)
        {
            state.Add(p.GetState());
        }

        return state;
    }

    public void SetState(List<PlayerState> ps)
    {
        foreach (PlayerState s in ps)
        {
            Player p = GetPlayer(s.id);

            if (p != null)
            {
                p.controlledEntities = s.controlledEntities;
            }
        }
    }
}

[Serializable]
public class Player {

    // ID is player's connection id
    public uint id;
    public string username;

    public List<int> controlledEntities;

    public Player()
    {

    }

    public Player(PlayerData pd)
    {
        id = pd.id;
        username = pd.username;
        controlledEntities = pd.controlledEntities;
    }

    public bool IsPlaying()
    {
        return controlledEntities.Count > 0;
    }

    public bool IsAlive()
    {
        return false;
    }

    public PlayerData GetData()
    {
        return new PlayerData(id, username, controlledEntities);
    }

    public PlayerState GetState()
    {
        return new PlayerState(id, controlledEntities);
    }

    public void SetState(PlayerState ps)
    {
        controlledEntities = ps.controlledEntities;
    }    
}

[Serializable]
public class PlayerData
{
    public uint id;
    public string username;

    public List<int> controlledEntities;

    public PlayerData ()
    {

    }

    public PlayerData(uint id, string username, List<int> controlledEntities)
    {
        this.id = id;
        this.username = username;
        this.controlledEntities = controlledEntities;
    }
}

[Serializable]
public class PlayerState
{
    public uint id;
    public List<int> controlledEntities;

    public PlayerState()
    {

    }

    public PlayerState(uint id, List<int> controlledEntities)
    {
        this.id = id;
        this.controlledEntities = controlledEntities;
    }
}

