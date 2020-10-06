using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


enum AnimalState
{
    Dead,
    Idle,
    SearchingForFood,
    SearchingForDrinks,
    IsDrinking,
    IsEating,
    LookingForPartnerToMate,
    Mating
}

public class AnimalUnit : MonoBehaviour
{
    [SerializeField]
    private GameObject animalPrefab;

    [SerializeField]
    private AstarAIMovement aiMovement;

    [SerializeField]
    private Color colorRandomOne = Color.white;

    [SerializeField]
    private Color colorRandomTwo = Color.white;  

    [SerializeField]
    private float age = 1.0f;

    [SerializeField]
    private float randomWanderDistance = 25.0f;

    [SerializeField]
    private float sizeRandomStart = 0.75f;

    [SerializeField]
    private float sizeRandomEnd = 1.0f;

    [SerializeField]
    private float sensorsRangeRandomStart = 7.5f;

    [SerializeField]
    private float sensorsRangeRandomEnd = 10.0f;

    [SerializeField]
    private float thirstIncreasePerSeconds = 0.5f;

    [SerializeField]
    private float hungerIncreasePerSeconds = 0.5f;

    [SerializeField]
    private float reproductionIncreasePerSeconds = 0.5f;

    [SerializeField]
    private float thirstLevelThreshold = 20.0f;

    [SerializeField]
    private float hungerLevelThreshold = 20.0f;

    [SerializeField]
    private float reproductionLevelThreshold = 20.0f;

    [SerializeField]
    private float thirstRegenarationRate = 10.0f;

    [SerializeField]
    private float hungerRegenarationRate = 10.0f;

    [SerializeField]
    private float reproductionRegenarationRate = 10.0f;

    [SerializeField]
    private float childThirstIncreasePerSeconds = 0.25f;

    [SerializeField]
    private float childHungerIncreasePerSeconds = 0.25f;

    [SerializeField]
    private float childReproductionIncreasePerSeconds = 0.0f;

    [SerializeField]
    private float childThirstLevelThreshold = 10.0f;

    [SerializeField]
    private float childHungerLevelThreshold = 10.0f;


    [SerializeField]
    private float childThirstRegenarationRate = 5.0f;

    [SerializeField]
    private float childHungerRegenarationRate = 5.0f;


    [SerializeField]
    private float pregnacyChance = 0.33f;

    [SerializeField]
    private float pregnacyDurationStart = 20.0f;

    [SerializeField]
    private float pregnacyDurationEnd = 30.0f;

    [SerializeField]
    private float mutationChance = 0.1f;

    [SerializeField]
    private float mutationAmount = 0.5f;

    [SerializeField]
    private LayerMask drinkLayerMask;

    [SerializeField]
    private LayerMask foodLayerMask;

    [SerializeField]
    private LayerMask animalLayerMask;

    [SerializeField]
    private AnimalState aiState;

    [SerializeField]
    private Color animalColor = Color.white;

    [SerializeField]
    private float size;

    [SerializeField]
    private float sensorsRange;

    [SerializeField]
    private float defaultSensorsRange;

    [SerializeField]
    private float thirstLevel;

    [SerializeField]
    private float hungerLevel;

    [SerializeField]
    private float reproductionLevel;

    [SerializeField]
    private float damageLevel = 0.0f;

    public bool CanEat { get; set; }
    public bool CanDrink { get; set; }
    public bool CanMate { get; set; }
    public bool IsFemale { get; set; }
    public bool IsPregnant { get; set; }

    public bool IsInitialised { get; set; }
    public bool IsMovmentInitialised { get; set; }
    public AnimalUnit Father { get => father; set => father = value; }
    public AnimalUnit Mother { get => mother; set => mother = value; }
    public float SensorsRange { get => sensorsRange; set => sensorsRange = value; }
    public Color ColorOne { get => colorRandomOne; set => colorRandomOne = value; }
    public Color ColorTwo { get => colorRandomTwo; set => colorRandomTwo = value; }
    public float Size { get => size; set => size = value; }
    public float DefaultSensorsRange { get => defaultSensorsRange; set => defaultSensorsRange = value; }
    public float Age { get => age; set => age = value; }
    public float ReproductionLevel { get => reproductionLevel; set => reproductionLevel = value; }
    public AnimalUnit NearestPartner { get => nearestPartner; set => nearestPartner = value; }

    private Collider nearestCollider = null;
    AnimalUnit nearestPartner = null;

    private AnimalUnit father;
    private AnimalUnit mother;
    private MaterialPropertyBlock propBlock;

    private AnimalUnit childFather;

    

    // Start is called before the first frame update
    void Start()
    {
        //reuse this if you are generating many
        //aiMovement.SetNewRandomTarget(10.0f);

        //aiMovement.SetFollowTarget(PickRandomPoint(randomWanderDistance));

        propBlock = new MaterialPropertyBlock();

        thirstLevel = 0.0f;
        hungerLevel = 0.0f;
        ReproductionLevel = 0.0f;

        if(Father != null && Mother != null)
        {
            InitAnimalWithParentValues();
        }
        else
        {
            InitAnimalWithRandomValues();
        }

       

        InvokeRepeating("Ageing", 0.0f, 1.0f);
        IsInitialised = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsInitialised && IsMovmentInitialised)
        {
            UpdateAnimalLevels();
            UpdateState();
            if (aiState == AnimalState.Dead)
            {
                CancelInvoke("Ageing");
            }
            else if (aiState == AnimalState.Idle)
            {
                if (aiMovement.ReachedEndOfPath || aiMovement.IsNotMoving())
                {
                    aiMovement.MoveToPosition(PickRandomPoint(randomWanderDistance));
                }
            }
            else if (aiState == AnimalState.SearchingForDrinks)
            {
                SearchAndGoToObjectInRangeOfTheAnimalSenses(drinkLayerMask);
            }
            else if (aiState == AnimalState.SearchingForFood)
            {
                SearchAndGoToObjectInRangeOfTheAnimalSenses(foodLayerMask);
            }
            else if (aiState == AnimalState.IsDrinking)
            {
                Drink();
            }
            else if (aiState == AnimalState.IsEating)
            {
                Eat();
            }
            else if (aiState == AnimalState.LookingForPartnerToMate)
            {
                if (!IsFemale)
                {
                    SearchAndFollowFemaleAnimal();
                }
                else
                {
                    SearchAndFollowMaleAnimal();
                }
            }
            else if (aiState == AnimalState.Mating)
            {
                Mate();
            }
        }
        transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);
       // transform.position = new Vector3(transform.position.x, 1.0f, transform.position.z);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Animal")
        {
            AnimalUnit animalUnit = collider.GetComponent<AnimalUnit>();
            if (!(this.IsFemale) && animalUnit.IsFemale)
            {
                CanMate = true;
                NearestPartner = animalUnit;
            }
            else if(this.IsFemale && !(animalUnit.IsFemale))
            {
                CanMate = true;
                NearestPartner = animalUnit;
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.tag == "Animal")
        {
            AnimalUnit animalUnit = collider.GetComponent<AnimalUnit>();
            if (NearestPartner == animalUnit)
            {
                CanMate = false;
                NearestPartner = null;
            }
           
        }
    }

    private void UpdateAnimalLevels()
    {
        if (Age < 1.0f)
        {
            if (!(aiState == AnimalState.IsEating))
            {
                hungerLevel += childHungerIncreasePerSeconds * Time.deltaTime;
                if(hungerLevel >= 100.0f)
                {
                    damageLevel += 1.0f * Time.deltaTime;
                }
            }
            if (!(aiState == AnimalState.IsDrinking))
            {
                thirstLevel += childThirstIncreasePerSeconds * Time.deltaTime;
                if (thirstLevel >= 100.0f)
                {
                    damageLevel += 1.0f * Time.deltaTime;
                }
            }
            if (!(aiState == AnimalState.Mating))
            {
                ReproductionLevel += childReproductionIncreasePerSeconds * Time.deltaTime;
            }

        }
        else
        {
            if (!(aiState == AnimalState.IsEating))
            {
                hungerLevel += hungerIncreasePerSeconds * Time.deltaTime;
                if (hungerLevel >= 100.0f)
                {
                    damageLevel += 0.8f * Time.deltaTime;
                }
            }
            if (!(aiState == AnimalState.IsDrinking))
            {
                thirstLevel += thirstIncreasePerSeconds * Time.deltaTime;
                if (thirstLevel >= 100.0f)
                {
                    damageLevel += 0.8f * Time.deltaTime;
                }
            }
            if (!(aiState == AnimalState.Mating))
            {
                ReproductionLevel += reproductionIncreasePerSeconds * Time.deltaTime;
            }
        }
    }

    private void UpdateState()
    {
        if(!(aiState == AnimalState.Dead))
        {
            if (damageLevel >= 100.0f && !(aiState == AnimalState.Dead))
            {
                aiState = AnimalState.Dead;
                Death();
            }
            else if (Age < 1.0f)
            {
                if (thirstLevel > childThirstLevelThreshold && !CanDrink)
                {
                    aiState = AnimalState.SearchingForDrinks;
                }
                else if (CanDrink && thirstLevel > 0.0f)
                {
                    aiState = AnimalState.IsDrinking;
                }
                else if (hungerLevel > childHungerLevelThreshold && !CanEat)
                {
                    aiState = AnimalState.SearchingForFood;
                }
                else if (CanEat && hungerLevel > 0.0f)
                {
                    aiState = AnimalState.IsEating;
                }
                else
                {
                    aiState = AnimalState.Idle;
                }
            }
            else
            {
                if (thirstLevel > thirstLevelThreshold && !CanDrink)
                {
                    aiState = AnimalState.SearchingForDrinks;
                }
                else if (CanDrink && thirstLevel > (thirstLevelThreshold / 3.0f))
                {
                    aiState = AnimalState.IsDrinking;
                }
                else if (hungerLevel > hungerLevelThreshold && !CanEat)
                {
                    aiState = AnimalState.SearchingForFood;
                }
                else if (CanEat && hungerLevel > (hungerLevelThreshold / 3.0f))
                {
                    aiState = AnimalState.IsEating;
                }
                else if (ReproductionLevel > reproductionLevelThreshold && !CanMate)
                {
                    aiState = AnimalState.LookingForPartnerToMate;
                }
                else if (CanMate && ReproductionLevel > reproductionLevelThreshold)
                {
                    aiState = AnimalState.Mating;
                }
                else
                {
                    aiState = AnimalState.Idle;
                }
            }
        }
       
    }

    private void SearchAndFollowMaleAnimal()
    {
        List<AnimalUnit> potentialPartners = new List<AnimalUnit>();
        Vector3 p1 = transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(p1, SensorsRange, animalLayerMask);
        foreach (Collider hitCollider in hitColliders)
        {
            if (!(hitCollider.GetComponent<AnimalUnit>().IsFemale))
            {
                if (hitCollider.GetComponent<AnimalUnit>() != this)
                {
                    potentialPartners.Add(hitCollider.GetComponent<AnimalUnit>());
                }
            }
        }

        float nearestDistance = float.MaxValue;
        float distance = 0.0f;
        NearestPartner = null;

        foreach (AnimalUnit partner in potentialPartners)
        {
            distance = Vector3.Distance(transform.position, partner.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                NearestPartner = partner;
            }
        }

        if (NearestPartner == null)
        {
            if (aiMovement.ReachedEndOfPath)
            {
                aiMovement.MoveToPosition(PickRandomPoint(SensorsRange));
            }
        }
        else
        {
            aiMovement.FollowTarget(NearestPartner.transform);
        }

    }

    private void SearchAndFollowFemaleAnimal()
    {
        List<AnimalUnit> potentialPartners = new List<AnimalUnit>();
        Vector3 p1 = transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(p1, SensorsRange, animalLayerMask);
        foreach (Collider hitCollider in hitColliders)
        {
           if(hitCollider.GetComponent<AnimalUnit>().IsFemale)
            {
                if(hitCollider.GetComponent<AnimalUnit>() != this)
                {
                    potentialPartners.Add(hitCollider.GetComponent<AnimalUnit>());
                }
            }
        }

        float nearestDistance = float.MaxValue;
        float distance = 0.0f;
        AnimalUnit nearestPartner = null;

        foreach (AnimalUnit partner in potentialPartners)
        {
            distance = Vector3.Distance(transform.position, partner.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPartner = partner;
            }
        }

        if (nearestPartner == null)
        {
            if (aiMovement.ReachedEndOfPath)
            {
                aiMovement.MoveToPosition(PickRandomPoint(SensorsRange));
            }
        }
        else
        {
            aiMovement.FollowTarget(nearestPartner.transform);
        }

    }

    private void SearchAndGoToObjectInRangeOfTheAnimalSenses(LayerMask objectLayer)
    {
        Vector3 p1 = transform.position;
        float nearestDistance = float.MaxValue;
        float distance = 0.0f;
        nearestCollider = null;

        Collider[] hitColliders = Physics.OverlapSphere(p1, SensorsRange, objectLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            distance = Vector3.Distance(transform.position, hitCollider.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCollider = hitCollider;
            }
        }

        if (nearestCollider == null)
        {
            if (aiMovement.ReachedEndOfPath)
            {
                aiMovement.MoveToPosition(PickRandomPoint(SensorsRange));
            }
        }
        else
        {
            aiMovement.MoveToPosition(nearestCollider.gameObject.transform.position);
        }
    }

    private void Drink()
    {
        if(Age < 1.0f)
        {
            thirstLevel -= childThirstRegenarationRate * Time.deltaTime;
        }
        else
        {
            thirstLevel -= thirstRegenarationRate * Time.deltaTime;
        }
        
    }

    private void Eat()
    {
        if (Age < 1.0f)
        {
            hungerLevel -= childHungerRegenarationRate * Time.deltaTime;
        }
        else
        {
            hungerLevel -= hungerRegenarationRate * Time.deltaTime;
        }
        
    }

    public void Mate()
    {
        if(!IsPregnant && IsFemale)
        {
            if (RandomWrapper.RandDouble() < pregnacyChance)
            {
                Invoke("SpawnChild", UnityEngine.Random.Range(pregnacyDurationStart, pregnacyDurationEnd));
                IsPregnant = true;
                childFather = NearestPartner;
            }
        }
        ReproductionLevel = 0.0f;
        NearestPartner.NearestPartner = this;
        if (NearestPartner.ReproductionLevel > 0.0f )
        {
            NearestPartner.Mate();
        } 
    }


    private void Ageing()
    {
        Age += (1.0f / 60.0f);
        if (Age < 1.0f)
        {
            sensorsRange = Age * DefaultSensorsRange;
        }
        else
        {
            sensorsRange = DefaultSensorsRange;
        }
    }

    private void SpawnChild()
    {
        GameObject child = Instantiate(animalPrefab);
        AnimalUnit childUnit = child.GetComponent<AnimalUnit>();
        childUnit.Father = childFather;
        childUnit.Mother = this;
        childUnit.Age = 0.0f;
        childUnit.transform.position = PickRandomPoint(2.5f);
        childUnit.IsInitialised = false;
        IsPregnant = false;
    }

    private void Death()
    {
        aiMovement.StopAgent();
        GetComponentInChildren<Renderer>().GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", Color.red);
        GetComponentInChildren<Renderer>().SetPropertyBlock(propBlock);
        CancelInvoke("Ageing");
    }

    void InitAnimalWithRandomValues()
    {
        IsFemale = (RandomWrapper.RandDouble() < 0.5f) ? true : false;
        DefaultSensorsRange = UnityEngine.Random.Range(sensorsRangeRandomStart, sensorsRangeRandomEnd);
        animalColor = Color.Lerp(ColorOne, ColorTwo, (float)RandomWrapper.RandDouble());
        Size = UnityEngine.Random.Range(sizeRandomStart, sizeRandomEnd);
        Material mat = Instantiate(GetComponent<Renderer>().material);
        GetComponent<Renderer>().GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", animalColor);
        GetComponent<Renderer>().SetPropertyBlock(propBlock);
        transform.localScale = new Vector3(Size, Size, Size);
        aiState = AnimalState.Idle;
    }

    void InitAnimalWithParentValues()
    {
        IsFemale = (RandomWrapper.RandDouble() < 0.5f) ? true : false;
        DefaultSensorsRange = InheritTrait(father.DefaultSensorsRange, mother.DefaultSensorsRange);
        animalColor = InheritTrait(father.animalColor, mother.animalColor);
        Size = InheritTrait(father.Size, mother.Size);
        GetComponent<Renderer>().GetPropertyBlock(propBlock);
        propBlock.SetColor("_BaseColor", animalColor);
        GetComponent<Renderer>().SetPropertyBlock(propBlock);
        transform.localScale = new Vector3(Size, Size, Size);
        aiState = AnimalState.Idle;
    }

    Vector3 PickRandomPoint(float radius)
    {
        var point = UnityEngine.Random.insideUnitSphere * radius;
        point.y = 0;
        point += gameObject.transform.position;
        return point;
    }

    float InheritTrait(float father, float mother)
    {
        float trait = (RandomWrapper.RandDouble() < 0.5f) ? mother : father;

        if(RandomWrapper.RandDouble() < mutationChance)
        {
           
            float mutateAmount = GetRandomGaussican() * mutationAmount;
            Debug.Log("Mutated Trait: " + mutateAmount);
            trait += mutateAmount;
        }

        return trait;
    }

    Color InheritTrait(Color father, Color mother)
    {
        Color trait = (RandomWrapper.RandDouble() < 0.5f) ? mother : father;

        if (RandomWrapper.RandDouble() < mutationChance)
        {
            float mutateAmountR = GetRandomGaussican() * mutationAmount;
            float mutateAmountG = GetRandomGaussican() * mutationAmount;
            float mutateAmountB = GetRandomGaussican() * mutationAmount;
            trait.r += mutateAmountR;
            trait.g += mutateAmountG;
            trait.b += mutateAmountB;
        }

        return trait;
    }

    float GetRandomGaussican()
    {
        double u1 = 1.0 - RandomWrapper.RandDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - RandomWrapper.RandDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)

        return (float)randStdNormal;
    }

    float GetRandomGaussican(float mean, float stdDev)
    {
        double u1 = 1.0 - RandomWrapper.RandDouble(); //uniform(0,1] random doubles
        double u2 = 1.0 - RandomWrapper.RandDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
        double randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

        return (float)randNormal;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Color gizmoColor = Color.green;
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, SensorsRange);
        Gizmos.DrawLine(transform.position, transform.forward);
        gizmoColor = Color.red;
        Gizmos.color = gizmoColor;
    }
}
