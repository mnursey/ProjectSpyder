using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType { SOLDIER, JEEP, MG_JEEP, TANK, ANTI_TANK, MED_TRUCK, RAM_TRUCK, MECH, BUNKER, SHIELD_TANK};

public enum EntityManagerMode { CLIENT, SERVER };

public class EntityManager : MonoBehaviour
{
    public ushort idCounter = 0;

    public List<IEntity> entities = new List<IEntity>();

    public List<GameObject> entityPrefabs = new List<GameObject>();

    public static EntityManager Instance;

    public EntityManagerMode mode;

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

        foreach(IEntity entity in entities.ToArray())
        {
            RemoveEntity(entity);
        }

        entities = new List<IEntity>();
    }

    public IEntity GetEntity(ushort id)
    {
        return entities.Find(x => x.id == id);
    }

    public IEntity GetEntity(GameObject g)
    {
        return entities.Find(x => x.gameObject == g);
    }

    void DisableServerSideEntityLogic(GameObject g)
    {
        foreach(Rigidbody r in g.GetComponentsInChildren<Rigidbody>())
        {
            r.isKinematic = true;
        }

        /*
        foreach (VehicleController vc in g.GetComponentsInChildren<VehicleController>())
        {
            vc.enabled = false;
        }

        foreach (VehicleAI va in g.GetComponentsInChildren<VehicleAI>())
        {
            va.enabled = false;
        }
        */

        foreach (LerpController lc in g.GetComponentsInChildren<LerpController>())
        {
            lc .enabled = true;
            lc.updateRate = ServerGameRunner.entityStateSendRate;
            lc.UpdateTargets(g.transform.position, g.transform.rotation);
        }
    }

    public IEntity CreateEntity(EntityType type)
    {
        IEntity entity = new GenericEntity();
        entity.entityPrefabIndex = (ushort)type;

        entity.id = ++idCounter;

        GameObject g = Instantiate(entityPrefabs[(ushort)type]);
        entity.gameObject = g;

        entities.Add(entity);

        if(mode == EntityManagerMode.CLIENT)
        {
            DisableServerSideEntityLogic(g);
        }

        return entity;
    }

    public IEntity CreateEntity(EntityData ed)
    {
        IEntity entity = new GenericEntity();
        entity.entityPrefabIndex = ed.entityPrefabIndex;

        entity.id = ed.id;
        entity.health = ed.health;

        GameObject g = Instantiate(entityPrefabs[ed.entityPrefabIndex]);
        entity.gameObject = g;

        g.transform.position = ed.pos.GetValue();
        g.transform.eulerAngles = ed.rot.GetValue();

        entities.Add(entity);

        if (mode == EntityManagerMode.CLIENT)
        {
            DisableServerSideEntityLogic(g);
        }

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
            UpdateEntityPosRot(e, ed.pos.GetValue(), Quaternion.Euler(ed.rot.GetValue()));
            e.health = ed.health;
        }
    }

    public void UpdateEntity(IEntity entity, EntityState es)
    {
        IEntity e = GetEntity(es.id);
        if (e != null)
        {
            // Update entity
            UpdateEntityPosRot(e, es.pos.GetValue(), Quaternion.Euler(es.rot.GetValue()));
            e.health = es.health;
            e.attackTarget = es.attackTarget;

            IUnit unit = e.gameObject.GetComponent<IUnit>();
            if(unit != null)
            {
                if(es.shot)
                {
                    // TODO
                    // IF SHOT THIS FRAME SPAWN SHOOTING VISUALS
                }

                // Update unit health 
                e.gameObject.GetComponent<VehicleController>().HP = e.health;

                // Update attack target on vehicle logic
                IEntity attackTarget = GetEntity(e.attackTarget);
                if(attackTarget != null)
                {
                    Debug.Log("found entity attack target " + attackTarget);
                    IUnit targetUniit = attackTarget.gameObject.GetComponent<IUnit>();
                    if(targetUniit != null)
                    {
                        unit.SetAttackTarget(targetUniit);
                    }
                }
            }
        }
    }

    public void UpdateEntityPosRot(IEntity e, Vector3 pos, Quaternion rot)
    {
        LerpController lc = e.gameObject.GetComponent<LerpController>();

        if(lc == null)
        {
            e.gameObject.transform.position = pos;
            e.gameObject.transform.rotation = rot;
        } else
        {
            lc.UpdateTargets(pos, rot);
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
    ushort health { get; set; }
    ushort attackTarget { get; set; }

    bool shot { get; set; }

    EntityData GetData();
    EntityState GetState();}

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

    private ushort _health = 1;
    public ushort health
    {
        get => _health;
        set => _health = value;
    }

    private bool _shot;
    public bool shot
    {
        get => _shot;
        set => _shot = value;
    }

    private ushort _attackTarget;
    public ushort attackTarget
    {
        get => _attackTarget;
        set => _attackTarget = value;
    }

    public EntityData GetData()
    {
        return new EntityData(_id, _entityPrefabIndex, _gameObject.transform.position, _gameObject.transform.eulerAngles, _health);
    }

    public EntityState GetState()
    {
        // Update unit health 
        int HP = _gameObject.GetComponent<VehicleController>().HP;
        if (HP < 0) HP = 0; 

        _health = (ushort)HP;

        IUnit u = _gameObject.GetComponent<IUnit>();

        if(u != null)
        {
            IUnit a = u.GetAttackTarget();
            if(a != null)
            {
                GameObject ag = a.GetGameObject();
                IEntity ae = EntityManager.Instance.GetEntity(ag);
                if(ae != null)
                {
                    _attackTarget = ae.id;
                }
            }
        }

        EntityState s = new EntityState(_id, _gameObject.transform.position, _gameObject.transform.eulerAngles, _health, _attackTarget);

        s.shot = _shot;
        _shot = false;

        return s;
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
    public ushort health;
    public EntityData () { }

    public EntityData(ushort id, ushort entityPrefabIndex, Vector3 pos, Vector3 rot, ushort health)
    {
        this.id = id;
        this.entityPrefabIndex = entityPrefabIndex;
        this.pos = new SVector3(pos);
        this.rot = new SVector3(rot);
        this.health = health;
    }
}

// Only entity properties to update clients about what is going on
[Serializable]
public class EntityState
{
    public ushort id;

    public SVector3 pos;
    public SVector3 rot;
    public ushort health;
    public ushort attackTarget;
    public bool shot;

    public EntityState() { }

    public EntityState(ushort id, Vector3 pos, Vector3 rot, ushort health, ushort attackTarget)
    {
        this.id = id;
        this.pos = new SVector3(pos);
        this.rot = new SVector3(rot);
        this.health = health;
        this.attackTarget = attackTarget;
    }
}