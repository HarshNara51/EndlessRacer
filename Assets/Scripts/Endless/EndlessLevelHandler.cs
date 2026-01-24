using System.Collections;
using UnityEngine;

// FIX 1: Added ": MonoBehaviour" so Unity knows this is a component
public class EndlessLevelHandler : MonoBehaviour
{
    [SerializeField]
    GameObject[] sectionsPrefabs;

    // FIX 2: Added "new" keyword to create the array
    GameObject[] sectionsPool = new GameObject[20];

    // FIX 2: Added "new" keyword here too
    GameObject[] sections = new GameObject[10];

    Transform playerCarTransform;

    WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);

    const float sectionLength = 26;

    void Start()
    {
        // FIX 3: Changed "FindGameObjects..." (Plural) to "FindGameObject..." (Singular)
        playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int prefabIndex = 0;

        //Create a pool for our endless sections
        for (int i = 0; i < sectionsPool.Length; i++)
        {
            sectionsPool[i] = Instantiate(sectionsPrefabs[prefabIndex]);
            
            // FIX 4: Corrected syntax. You call .SetActive() ON the object.
            sectionsPool[i].SetActive(false);

            prefabIndex++;

            //Loop the prefab index if we run out of prefabs
            if (prefabIndex > sectionsPrefabs.Length - 1)
                prefabIndex = 0;
        }

        //Add the first sections to the road
        for (int i = 0; i < sections.Length; i++)
        {
            //Get a random section
            GameObject randomSection = GetRandomSectionFromPool();

            //Move it into position and set it to active
            randomSection.transform.position = new Vector3(sectionsPool[i].transform.position.x, 0, i * sectionLength);
            randomSection.SetActive(true);

            //Set the section in the array
            sections[i] = randomSection;
        }

        StartCoroutine(UpdateLessOftenCO());
    }

    IEnumerator UpdateLessOftenCO()
    {
        while(true)
        {
            UpdateSectionPositions();
            yield return waitFor100ms;
        }
    }

    void UpdateSectionPositions()
    {
        for (int i = 0; i < sections.Length; i++)
        {
            //Check if section is too far behind
            if (sections[i].transform.position.z - playerCarTransform.position.z < -sectionLength)
            {
                //Store the position of the section and disable it
                Vector3 lastSectionPosition = sections[i].transform.position;
                sections[i].SetActive(false);

                //Get new section and enable it & move it forward
                sections[i] = GetRandomSectionFromPool();

                //Move the new section into place and active it
                // FIX 5: Changed "sectionsLength" (undefined variable) to "sections.Length" (the array count)
                sections[i].transform.position = new Vector3(lastSectionPosition.x, 0, lastSectionPosition.z + sectionLength * sections.Length);
                sections[i].SetActive(true);

            }
        }
    }

    GameObject GetRandomSectionFromPool()
    {
        //Pick a random index and hope that it is available
        int randomIndex = Random.Range(0, sectionsPool.Length);

        bool isNewSectionFound = false;

        while(!isNewSectionFound)
        {
            //Check if the section is not active, in that case we've found a section
            if(!sectionsPool[randomIndex].activeInHierarchy)
                isNewSectionFound = true;
            else
            {
                //If it was active we need to try to find another one so we increase the index
                randomIndex++;

                //Ensure that we loop around if we reach the end of the array
                if(randomIndex > sectionsPool.Length - 1)
                    randomIndex = 0;
            }
        }

        return sectionsPool[randomIndex];
    }
}