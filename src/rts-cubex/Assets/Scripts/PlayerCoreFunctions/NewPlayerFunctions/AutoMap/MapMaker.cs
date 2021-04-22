﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//NOTE: Create deposit spawner
//NOTE: Create playerBase and Borg spawner
//NOTE: Add some obstacles to empty hole fields
//NOTE: For starting point user: 6x6 platform

public class MapMaker : MonoBehaviour
{
    //[SerializeField] GameObject platform; //platform cube
    [Header("Main cnf. p.")]
    [SerializeField] GameObject pCube; //platform cube
    [SerializeField] GameObject walls;
    [SerializeField] GameObject areaWall;
    [SerializeField] GameObject holeCube;
    [SerializeField] GameObject moundCube;
    [SerializeField] GameObject moundNormal;
    [SerializeField] GameObject energonDeposit;
    [Tooltip("Platform Width in Cubes")]
    [Range(6, 6)] //For locking values in inspector
    [SerializeField] int pWidth = 6;  
    [Tooltip("Platform Height in Cubes")] 
    [Range(6, 6)] //For locking values in inspector
    [SerializeField] int pHeight = 6; 
    [Tooltip("Height of the walls")]
    [SerializeField] float wHeight = 4;
    [Header("Hole generation cnf.")]
    [Range(1, 2)]
    [SerializeField] int holeCount = 2;
    [Header("Mound generation cnf")]
    [Range(1, 2)]
    [SerializeField] int moundCount = 2;
    [Tooltip("Transformation offset XYZ")] 
    [SerializeField] float tOffset = 50f;
    [SerializeField] bool generateNavMesh = false;
    [SerializeField] bool generateWalls = false;
    [SerializeField] bool generateGround = false;
    [SerializeField] bool createAreaWalls = false;
    [SerializeField] bool createHoles = false;
    [SerializeField] bool createMounds = false;
    [SerializeField] bool spawnEDeposits = false;
    //private float x = 0f, y = 0f, z = 0f;
    //private GameObject sPlatform;
    //Object sub groups
    private GameObject levelMap;
    private GameObject groundGrp;
    private GameObject wallAreaGrp;
    private GameObject holeGrp;
    private GameObject moundGrp;
    private GameObject energonGrp;
    // private List<int> rHoleIndexes = new List<int>(); //For first lane
    //  private List<int> rEHoleIndexes = new List<int>(); //For end lane
    private int oldHoleIndex = 0;
    private int oldMoundIndex = 1;
    private bool holeLaneSwitch = false;
    private bool moundLaneSwitch = false;
    private Helper help = new Helper();
   // private GameObject startCube;
    private List<List<GameObject>> sCubes = new List<List<GameObject>>(); //list of spawned cubes
    private List<GameObject> mounds = new List<GameObject>();
    void Start()
    {
       
        //Create platform object groups
        levelMap = new GameObject("LevelMap"); //Main paltform group
        
        levelMap.transform.position = Vector3.zero;
        
        if (generateGround)
        {
            groundGrp = new GameObject("Ground");
            groundGrp.transform.position = Vector3.zero;
            for (float posZIndex = 0f; posZIndex > -(pHeight * tOffset); posZIndex -= tOffset)
            {
                var cList = new List<GameObject>();
                var countW = 0;
                for(float posXIndex = 0f; posXIndex < (pWidth * tOffset); posXIndex += tOffset)
                {                     
                    var scube = Instantiate(pCube, new Vector3(posXIndex, 0f, posZIndex), pCube.transform.rotation);
                    scube.transform.parent = groundGrp.transform;
                    //scube.SetActive(true);
                    //countW++;
                    cList.Add(scube);
                }
                sCubes.Add(cList);
              //  if (sCubes[0].Count >= pWidth) print("PRow created!");
               
            }
            groundGrp.transform.parent = levelMap.transform;
        }

        //platform.transform.position = new Vector3(-((pWidth * tOffset) / 2), 0f, ((pHeight * tOffset) / 2));
        //For centering of platform rect in 0,0,0
        var newWidth = (pCube.transform.localScale.x * pWidth) / 2;
        var newHeight = (pCube.transform.localScale.x * pHeight) / 2;
        levelMap.transform.position = new Vector3(-(newWidth - newWidth / 4), 0f, (newHeight - newHeight / 4));
        //Build platform main wall
        if (generateWalls)
        {           
            var spawnedWalls = Instantiate(walls, Vector3.zero, walls.transform.rotation);
            spawnedWalls.transform.localScale = new Vector3((((pWidth * tOffset) / 4)) / 2 - 5.5f, 
                                                               spawnedWalls.transform.localScale.y * wHeight,
                                                               (((pHeight * tOffset) / 4)) / 2 - 5.5f);
            spawnedWalls.transform.position = new Vector3(spawnedWalls.transform.position.x, wHeight / 2, spawnedWalls.transform.position.z);
            spawnedWalls.transform.parent = levelMap.transform;
        }

        //Creating paltform obstacles
        if (createAreaWalls)
        {            
            wallAreaGrp = new GameObject("AreaWalls");
            wallAreaGrp.transform.position = Vector3.zero;
            bool rotateWall = false;
            int rIndex = 1;
            int cIndex = 1;
            float zOffset = 0f;
            for (int sCubeIndex = pHeight / 2 - 1; sCubeIndex >= 0; sCubeIndex--)
            {
                if (sCubeIndex == pHeight / 2 - 1)
                {
                    float xOffset = 0f; //For getting away from center
                    zOffset = pCube.transform.localScale.x / 2;
                    for(int sColIndex = pWidth / 2 - 1; sColIndex >= 0; sColIndex--)
                    {
                        var _leftTransform = sCubes[sCubeIndex + 1][sColIndex].transform;
                        var _rightTransform = sCubes[sCubeIndex + 1][sColIndex + cIndex].transform;
                        
                        if (sColIndex == pWidth / 2 - 1)
                        {
                            xOffset = pCube.transform.localScale.x / 2;
                        }
                        else
                        {
                            xOffset = 0f;
                        }

                        var _spawnWall1 = Instantiate(areaWall, _leftTransform.position + new Vector3(-xOffset, 0f, pCube.transform.localScale.x / 2), _leftTransform.rotation);
                        _spawnWall1.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                        
                        var _spawnWall2 = Instantiate(areaWall, _rightTransform.position + new Vector3(xOffset, 0f, pCube.transform.localScale.x / 2), _rightTransform.rotation);
                        _spawnWall2.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                        _spawnWall1.transform.parent = wallAreaGrp.transform;
                        _spawnWall2.transform.parent = wallAreaGrp.transform;

                        cIndex += 2;
                    }
                }
                else
                {
                    zOffset = 0f;
                }
                var leftTransform = sCubes[sCubeIndex][pWidth / 2 - 1].transform;
                var rightTransform = sCubes[sCubeIndex + rIndex][pWidth / 2 - 1].transform;
                var spawnWall1 = Instantiate(areaWall, leftTransform.position + new Vector3(pCube.transform.localScale.x / 2, 0f, zOffset), leftTransform.rotation);
                var spawnWall2 = Instantiate(areaWall, rightTransform.position + new Vector3(pCube.transform.localScale.x / 2, 0f, -zOffset), rightTransform.rotation);
                spawnWall1.transform.parent = wallAreaGrp.transform;
                spawnWall2.transform.parent = wallAreaGrp.transform;
                rIndex += 2;
               // print(rIndex);
            }            

            //Parenting to main map group
            wallAreaGrp.transform.parent = levelMap.transform;
        }
        if (createHoles)
        {
            var hList = new List<GameObject>();
            holeGrp = new GameObject("Platform holes");
            holeGrp.transform.position = Vector3.zero;
            var pFirstLane = sCubes[0];
            var pEndLane = sCubes[sCubes.Count - 1];
            var spawnIndexes = new List<int>();
            //NOTE: Since we are working with a square ground platform, first and last lanes will always be equal
            spawnIndexes.Add(pFirstLane.Count / 2 - 1);
            spawnIndexes.Add(pFirstLane.Count / 2);            
            
            if (spawnIndexes.Count > 0)
            {
                if(spawnIndexes.Count >= holeCount)
                {
                    for(int index = 0; index < holeCount; index++)
                    {
                        holeLaneSwitch = !holeLaneSwitch;
                        List<GameObject> pLane = new List<GameObject>();
                        //List<int> lIndexes = new List<int>();
                        //For random range generation
                        // var sValue = 0;
                        // var eValue = 0;
                        if (holeLaneSwitch)
                        {
                             //lIndexes = rHoleIndexes;
                             pLane = pFirstLane;
                            //  sValue = 0;
                            //  eValue = spawnIndexes.Count / 2;
                        }
                        else
                        {
                            //lIndexes = rEHoleIndexes;
                            pLane = pEndLane;
                            // sValue = spawnIndexes.Count / 2;
                            // eValue = spawnIndexes.Count;
                        }
                        var randPosIndex = Random.Range(0, spawnIndexes.Count);
                        // For generating unique random numbers
                        if (oldHoleIndex == randPosIndex)
                        {
                            while(oldHoleIndex == randPosIndex)
                            {
                                randPosIndex = Random.Range(0, spawnIndexes.Count);
                                if (oldHoleIndex != randPosIndex)
                                {      
                                   oldHoleIndex = randPosIndex;                            
                                   break; 
                                }
                            }
                        }
                        else
                        {
                             oldHoleIndex = randPosIndex;  
                        }
                        //rHoleIndexes.Add(randNum);
                      
                        var sPosLane = pLane[spawnIndexes[randPosIndex]];//hole spawn point
                        
                        var spawnedHole = Instantiate(holeCube, sPosLane.transform.position, sPosLane.transform.rotation);
                        pLane.Add(spawnedHole);
                        spawnedHole.transform.parent = holeGrp.transform;
                        //Removing existing platform model
                        pLane.Remove(sPosLane);
                        Destroy(sPosLane);
                    }
                    
                }
                else
                {
                    Debug.LogError("ERROR: Hole count is larger than spawn indexes set!");
                }
            }
            
            holeGrp.transform.parent = levelMap.transform;
            //var holeIndex = sCubex[Random]
        }
        //Mound generation
        if(createMounds)
        {
            //moundCube
            moundGrp = new GameObject("Mounds");
            moundGrp.transform.position = Vector3.zero;

            var leftLane = sCubes[(((sCubes.Count - 1) / 2) - 1)];
            var rightLane = sCubes[sCubes.Count - ((sCubes.Count - 1) / 2)];
            var spawnIndexes = new List<int>();
            spawnIndexes.Add(((leftLane.Count - 1) / 2) - 1);
            spawnIndexes.Add(leftLane.Count - ((leftLane.Count - 1) / 2));
            // spawnIndexes.Add(((leftLane.Count - 1) / 2) + 1);
            // spawnIndexes.Add();
            
            for(int mIndex = 0; mIndex < moundCount; mIndex++)
            {
                moundLaneSwitch = !moundLaneSwitch;
                List<GameObject> pLane = new List<GameObject>(); 
                List<GameObject> nPLane = new List<GameObject>(); 
                var mRotAngle = Vector3.zero; //Rotation angle on Yaxis
                if (moundLaneSwitch)
                {
                    pLane = leftLane;
                    nPLane = rightLane;
                    mRotAngle = new Vector3(0f, 90f, 0f);
                }
                else
                {
                    pLane = rightLane;
                    nPLane = leftLane;
                }
                var randPosIndex = Random.Range(0, spawnIndexes.Count);
                if (randPosIndex == oldMoundIndex)
                {
                    while(randPosIndex == oldMoundIndex)
                    {
                        randPosIndex = Random.Range(0, spawnIndexes.Count);
                        if (randPosIndex != oldMoundIndex)
                        {
                           oldMoundIndex = randPosIndex;
                           break; 
                        }
                    }
                }
                else
                {
                    oldMoundIndex = randPosIndex;
                }

                //Mound spawning
                var cPScale = pLane[spawnIndexes[randPosIndex]].transform.localScale;
                var mPos = pLane[spawnIndexes[randPosIndex]].transform.position + new Vector3(0f, cPScale.y, 0f);
                
                //Mound normal spawning pos
                //var nMIndex = spawnIndexes[randPosIndex] > 0 ? spawnIndexes[randPosIndex] - 1 : spawnIndexes[randPosIndex];
                var mNPos = nPLane[spawnIndexes[randPosIndex]].transform.position + new Vector3(0f, cPScale.y, 0f);
                
                var spawnedMound = Instantiate(moundCube, mPos, Quaternion.Euler(mRotAngle));
                mounds.Add(spawnedMound);

                var spawnedMoundN = Instantiate(moundNormal, mNPos, Quaternion.Euler(mRotAngle));
                //Parent to required sub group
                spawnedMound.transform.parent = moundGrp.transform;
                spawnedMoundN.transform.parent = moundGrp.transform;
            }
            moundGrp.transform.parent = levelMap.transform;
        }
        //Energon deposit spawning
        if (spawnEDeposits)
        {
            energonGrp = new GameObject("EnergonDeposits");
            energonGrp.transform.position = Vector3.zero;

            var rightLane = sCubes[((sCubes.Count - 1) / 2) - 2];
            var leftLane = sCubes[(sCubes.Count - 1) / 2];
            var offsetT = pCube.transform.localScale.x / 4;
            var endPosX = rightLane[(rightLane.Count / 2)].transform.position.x - 2 * pCube.transform.localScale.x;           
            
            //Right lane spawning
            //Spawning in the right region energon deposit
            for(float xPos = rightLane[0].transform.position.x; xPos < endPosX; xPos += offsetT)
            {        
                var ePos = new Vector3(xPos, pCube.transform.localScale.y + 1f, rightLane[(rightLane.Count / 2 - 1)].transform.position.z - 2 * offsetT);       
                var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                spawnedDeposit.transform.parent = energonGrp.transform;
            }

            //Spawning in the left region energon deposits
            endPosX = rightLane[(rightLane.Count - 1)].transform.position.x + pCube.transform.localScale.x;           
            for(float xPos = rightLane[rightLane.Count - 2].transform.position.x; xPos >= endPosX; xPos -= offsetT)
            {        
                var ePos = new Vector3(xPos, pCube.transform.localScale.y + 1f, rightLane[(rightLane.Count / 2 - 1)].transform.position.z - 2 * offsetT);       
                var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                spawnedDeposit.transform.parent = energonGrp.transform;
            }
            //--------------------------------
            //Left lane spawning
            //endPosX works with both lanes because platform is a square
            for(float xPos = leftLane[0].transform.position.x; xPos < endPosX; xPos += offsetT)
            {        
                var ePos = new Vector3(xPos, pCube.transform.localScale.y + 1f, leftLane[(leftLane.Count / 2 - 1)].transform.position.z - 10 * offsetT);       
                var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                spawnedDeposit.transform.parent = energonGrp.transform;
            }

            //Spawning in the left region energon deposits
            //FIX this-----------------------
            endPosX = leftLane[(leftLane.Count - 1)].transform.position.x + pCube.transform.localScale.x;           
            for(float xPos = leftLane[leftLane.Count - 2].transform.position.x; xPos >= endPosX; xPos -= offsetT)
            {        
                var ePos = new Vector3(xPos, pCube.transform.localScale.y + 1f, leftLane[(leftLane.Count / 2 - 1)].transform.position.z - 10 * offsetT);       
                var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                spawnedDeposit.transform.parent = energonGrp.transform;
            }
            energonGrp.transform.parent = levelMap.transform;
        }
        //NOTE: Generate NavMesh at the end
        if (generateNavMesh)
        {
            if(sCubes.Count > 0)
            {
                foreach(var crow in sCubes)
                {
                    foreach(var cube in crow)
                    {
                        if ((cube.name.ToLower()).Contains("hole"))
                        {
                            var iPlatform = help.getChildGameObjectByName(cube, "BWArea");
                            iPlatform.GetComponent<NavMeshSurface>().BuildNavMesh();
                        }                        
                        else
                        {
                            // if(cube.activeSelf)
                            // {
                                cube.GetComponent<NavMeshSurface>().BuildNavMesh();
                            //}
                        }
                    }
                }
            }
            if (mounds.Count > 0)
            {
                foreach(var m in mounds)
                {
                    m.GetComponent<MoundNavGen>().BuildNavMesh = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}