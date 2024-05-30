using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SectionGenerator : Parallax
{
    #region Serialized fields
    [SerializeField] private WeightedSection[] initialSections = null;

    [SerializeField] private WeightedSection[] potentialSections = null;

    [SerializeField] private SectionRepetitionConstraints[] sectionRepetitionConstraints = null;

    [SerializeField] private float levelWidth;

    [SerializeField] private float offset;

    [Header("Test")]
    [SerializeField] private VictoryUI victoryUI;
    #endregion

    #region Attributes
    private float currentLevelWidth;

    private List<Section> sectionSequence = new List<Section>();

    private List<SectionGeometry> sectionGeoSequence = new List<SectionGeometry>();


    private Dictionary<WeightedSection, Queue<SectionGeometry>> pools = new Dictionary<WeightedSection, Queue<SectionGeometry>>();

    private Dictionary<SectionGeometry, WeightedSection> instanceToPrefab = new Dictionary<SectionGeometry, WeightedSection>();

    private Queue<SectionGeometry> instances = new Queue<SectionGeometry>();
    #endregion

    #region API
    public float CurrentPos { get; private set; }
    #endregion

    #region Unity methods
    protected override void Awake()
    {
        base.Awake();

        Scrolling = true;

        CurrentPos = 0;

        InitialFill();

        if (Spawning)
            Fill();
    }

    private void Update()
    {
        if (Scrolling)
        {
            transform.position = transform.position + Vector3.left * speed * SpeedFactor * Time.deltaTime;

            CurrentPos = -transform.position.x;
            /*float f = speed * SpeedFactor;

            CurrentPos += f * Time.deltaTime;

            foreach (SectionGeometry geo in sectionGeoSequence)
            {
                geo.Move(f);
            }*/
        }

        if (Spawning)
            Fill();

        if (instances.TryPeek(out SectionGeometry props))
        {
            if (props.transform.position.x + props.Width < -5f)
            {
                instances.Dequeue();
                RemoveProps(props);
            }
        }


        if (CurrentPos >= levelWidth)
        {
            victoryUI.Show();
            Scrolling = false;
        }
    }
    #endregion

    #region Private
    private void InitialFill()
    {
        for (int i = 0; i < initialSections.Length; ++i)
        {
            SectionGeometry initialProps = Instantiate(initialSections[i].Section.Geometry.gameObject, transform).GetComponent<SectionGeometry>();
            initialProps.gameObject.transform.localPosition = Vector3.right * currentLevelWidth;

            Section instanciatedProps = initialSections[i].Section;
            sectionSequence.Add(instanciatedProps);
            sectionGeoSequence.Add(initialProps);
            //instances.Enqueue(initialProps);

            currentLevelWidth += initialProps.Width;
        }
    }

    private void Fill()
    {
        Debug.Log(CurrentPos + offset + ", " + currentLevelWidth);

        while (CurrentPos + offset >= currentLevelWidth)
        {
            Debug.Log("Fill");

            if (currentLevelWidth < levelWidth)
                AddSection();
            else
                InitialFill();
        }
    }

    private void AddSection()
    {
        WeightedSection newSection = GetRdWeightedSection();
        newSection.SetCooldown();

        SectionGeometry newGeo = InstantiateProps(newSection);
        newGeo.transform.localPosition = Vector3.right * currentLevelWidth;

        sectionSequence.Add(newSection.Section);
        sectionGeoSequence.Add(newGeo);
        instances.Enqueue(newGeo);


        foreach (WeightedSection potentialSection in potentialSections)
            potentialSection.DecreaseCooldown();

        currentLevelWidth += newGeo.Width;
    }

    private WeightedSection GetRdWeightedSection()
    {
        foreach (SectionRepetitionConstraints sectionRepetitionConstraint in sectionRepetitionConstraints)
        {
            if (sectionRepetitionConstraint.IsConstrainedActivityAllowed(sectionSequence))
            {
                potentialSections.First(x => x.Section == sectionRepetitionConstraint.Section).ForceCooldown();
            }
        }

        float totalWeights = potentialSections.Sum(x => x.GetWeightAt(sectionSequence[sectionSequence.Count - 1]));


        float rd = Random.value * totalWeights;

        for (int i = 0; i < potentialSections.Length; ++i)
        {
            float weightAt = potentialSections[i].GetWeightAt(sectionSequence[sectionSequence.Count - 1]);

            if (rd < weightAt)
                return potentialSections[i];

            rd -= weightAt;
        }

        return potentialSections.Last();
    }

    private SectionGeometry InstantiateProps(WeightedSection prefab)
    {
        if (pools.TryGetValue(prefab, out Queue<SectionGeometry> queue) && queue.Count > 0)
        {
            SectionGeometry instance = queue.Dequeue();
            instance.gameObject.SetActive(true);

            return instance;
        }

        SectionGeometry newInstance = Instantiate(prefab.Section.Geometry, transform);
        instanceToPrefab[newInstance] = prefab;

        return newInstance;
    }

    private void RemoveProps(SectionGeometry instance)
    {
        WeightedSection prefab = instanceToPrefab[instance];
        instance.gameObject.SetActive(false);

        if (!pools.TryGetValue(prefab, out Queue<SectionGeometry> queue))
        {
            queue = new Queue<SectionGeometry>();
            pools[prefab] = queue;
        }

        queue.Enqueue(instance);
    }

    /*private SectionGeometry InstantiateProps(WeightedSection weightedSection)
    {
        SectionGeometry newGeo = Instantiate(weightedSection.Section.Geometry.gameObject, transform).GetComponent<SectionGeometry>();

        return newGeo;
    }*/
    #endregion
}
