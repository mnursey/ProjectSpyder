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

    public List<EntityData> GetState()
    {
        List<EntityData> data = new List<EntityData>();

        foreach(IEntity e in entities)
        {
            data.Add(e.GetData());
        }

        return data;
    }

    public void SetState(List<EntityData> data)
    {
        foreach(EntityData ed in data)
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
}

[Serializable]
public class EntityData
{
    public ushort id;
    public ushort entityPrefabIndex;

    // Todo use these
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