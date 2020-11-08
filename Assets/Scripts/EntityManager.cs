using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public ushort idCounter = 0;

    public List<IEntity> entities = new List<IEntity>();

    public List<GameObject> entityPrefabs = new List<GameObject>();

    public static EntityManager Instance;

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        } else
        {
            Debug.LogError("More than one Entity Manager");
        }
    }

    public void Reset()
    {
        idCounter = 0;

        foreach(IEntity entity in entities)
        {
            RemoveEntity(entity);
        }

        entities = new List<IEntity>();
    }

    public IEntity GetEntity(ushort id)
    {
        return entities.Find(x => x.id == id);
    }

    public IEntity CreateEntity(ushort entityPrefabIndex)
    {
        IEntity entity = new GenericEntity();
        entity.entityPrefabIndex = entityPrefabIndex;

        entity.id = ++idCounter;

        GameObject g = Instantiate(entityPrefabs[entityPrefabIndex]);
        entity.gameObject = g;

        return entity;
    }

    public IEntity CreateEntity(EntityData ed)
    {
        IEntity entity = new GenericEntity();
        entity.entityPrefabIndex = ed.entityPrefabIndex;

        entity.id = ed.id;

        GameObject g = Instantiate(entityPrefabs[ed.entityPrefabIndex]);
        entity.gameObject = g;

        g.transform.position = ed.pos.GetValue();
        g.transform.eulerAngles = ed.rot.GetValue();

        return entity;
    }

    public void RemoveEntity(ushort id)
    {
        RemoveEntity(GetEntity(id));
    }

    public void RemoveEntity(IEntity entity)
    {
        if(entity != null)
        {
            entities.Remove(entity);
            Destroy(entity.gameObject);
        }
        else
        {
            Debug.LogError("Tried to remove null entity");
        }
    }

    public void UpdateEntity(EntityData ed)
    {
        UpdateEntity(GetEntity(ed.id), ed);
    }

    public void UpdateEntity(EntityState es)
    {
        UpdateEntity(GetEntity(es.id), es);
    }

    public void UpdateEntity(IEntity entity, EntityData ed)
    {
        IEntity e = GetEntity(ed.id);
        if (e == null)
        {
            // Create new entity
            CreateEntity(ed);
        } else
        {
            // Update entity
            e.gameObject.transform.position = ed.pos.GetValue();
            e.gameObject.transform.eulerAngles = ed.rot.GetValue();
        }
    }

    public void UpdateEntity(IEntity entity, EntityState es)
    {
        IEntity e = GetEntity(es.id);
        if (e != null)
        {
            // Update entity
            e.gameObject.transform.position = es.pos.GetValue();
            e.gameObject.transform.eulerAngles = es.rot.GetValue();
        }
    }

    public List<EntityState> GetState()
    {
        List<EntityState> data = new List<EntityState>();

        foreach(IEntity e in entities)
        {
            data.Add(e.GetState());
        }

        return data;
    }

    public void SetState(List<EntityState> state)
    {
        foreach(EntityState es in state)
        {
            UpdateEntity(es);
        } 
    }

    public List<EntityData> GetData ()
    {
        List<EntityData> data = new List<EntityData>();

        foreach (IEntity e in entities)
        {
            data.Add(e.GetData());
        }

        return data;
    }

    public void SetData(List<EntityData> data)
    {
        foreach (EntityData ed in data)
        {
            UpdateEntity(ed);
        }
    }
}

public interface IEntity
{
    GameObject gameObject { get; set; }
    ushort id { get; set; }
    ushort entityPrefabIndex { get; set; }

    EntityData GetData();
    EntityState GetState();
}

[Serializable]
public class GenericEntity : IEntity
{
    private GameObject _gameObject;
    public GameObject gameObject
    {
        get => _gameObject;
        set => _gameObject = value;
    }

    private ushort _id;
    public ushort id
    {
        get => _id;
        set => _id = value;
    }

    private ushort _entityPrefabIndex;
    public ushort entityPrefabIndex
    {
        get => _entityPrefabIndex;
        set => _entityPrefabIndex = value;
    }

    public EntityData GetData()
    {
        return new EntityData(_id, _entityPrefabIndex, _gameObject.transform.position, _gameObject.transform.eulerAngles);
    }

    public EntityState GetState()
    {
        return new EntityState(_id, _gameObject.transform.position, _gameObject.transform.eulerAngles);
    }
}

// All entitiy properties
[Serializable]
public class EntityData
{
    public ushort id;
    public ushort entityPrefabIndex;

    public SVector3 pos;
    public SVector3 rot;

    public EntityData () { }

    public EntityData(ushort id, ushort entityPrefabIndex, Vector3 pos, Vector3 rot)
    {
        this.id = id;
        this.entityPrefabIndex = entityPrefabIndex;
        this.pos = new SVector3(pos);
        this.rot = new SVector3(rot);
    }
}

// Only entity properties to update clients about what is going on
[Serializable]
public class EntityState
{
    public ushort id;

    public SVector3 pos;
    public SVector3 rot;

    public EntityState() { }

    public EntityState(ushort id, Vector3 pos, Vector3 rot)
    {
        this.id = id;
        this.pos = new SVector3(pos);
        this.rot = new SVector3(rot);
    }
}