﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

//NOTE: For starting point user: 6x6 platform!!
public class MapMaker : MonoBehaviour
{
    [Header("Map UI cnf.:")]    
    [SerializeField] GameObject gameHood;
    [SerializeField] float delayTime = 2f;
    [Header("Level map assets cnf.:")]   
    [SerializeField] GameObject mainCamera; 
    [SerializeField] GameObject pCube; //platform cube
    [SerializeField] GameObject walls;
    [SerializeField] GameObject areaWall;
    [SerializeField] GameObject holeCube;
    [SerializeField] GameObject moundCube;
    [SerializeField] GameObject moundNormal;
    [SerializeField] GameObject energonDeposit;
    [SerializeField] GameObject wallCube;
    [SerializeField] GameObject sPlayerBase;
    [SerializeField] GameObject sBorgBase;
    [SerializeField] GameObject levelSessionMgr;
    [SerializeField] GameObject lootBox;
    [Tooltip("Platform Width in Cubes")]
    [Range(6, 6)] //For locking values in inspector
    [SerializeField] int pWidth = 6;  
    [Tooltip("Platform Height in Cubes")] 
    [Range(6, 6)] //For locking values in inspector
    [SerializeField] int pHeight = 6; 
    [Tooltip("Height of the walls")]
    [Range(2, 4)]
    [SerializeField] float wallHeight = 4;
    [Header("Hole generation cnf.")]
    [Range(1, 2)]
    [SerializeField] int holeCount = 2;
    [Header("Mound generation cnf")]
    [Range(1, 2)]
    [SerializeField] int moundCount = 2;
    [Tooltip("Transformation offset XYZ")] 
    [SerializeField] float tOffset = 50f;
    [Header("Options cnf.:")]
    [SerializeField] bool generateNavMesh = false;
    [SerializeField] bool generateWalls = false;
    [SerializeField] bool createAreaWalls = false;
    [SerializeField] bool createHoles = false;
    [SerializeField] bool createMounds = false;
    [SerializeField] bool spawnEDeposits = false;
    [SerializeField] bool spawnGameBases = false;
    [SerializeField] bool spawnLootBoxes = false;
    [SerializeField] bool readyToBuildChallenge = false;
    [SerializeField] bool isTimeUnlimited = true;
    //Level main group
    private GameObject levelMap;
    //Level sub groups
    private GameObject groundGrp;
    private GameObject wallAreaGrp;
    private GameObject boundWalls;
    private GameObject holeGrp;
    private GameObject moundGrp;
    private GameObject energonGrp;
    private GameObject gSpawnMBaseGrp;
    private GameObject lootBoxGrp;
    
    //For randomization picking
    private int oldHoleIndex = 0;
    private int oldMoundIndex = 1;
    private int oldBoxType = 0;

    //For randomization switching
    private bool holeLaneSwitch = false;
    private bool moundLaneSwitch = false;
    private bool generateGround = true;
    private bool createLevel = true;    
    
    //For caching created level assets in scene
    //For ground
    private List<List<GameObject>> sCubes = new List<List<GameObject>>(); //list of spawned cubes
    
    public List<List<GameObject>> SCubes {get { return sCubes; }}
    public GameObject SPlayerBase { get { return sPlayerBase; }}
    public GameObject GameHood { get { return gameHood; }}
    public GameObject MainCamera { get { return mainCamera; }}
    //For mounds 
    private List<GameObject> mounds = new List<GameObject>();
    //For holes
    private List<GameObject> holes = new List<GameObject>();
    public bool ReadyToBuildChallenge { get { return readyToBuildChallenge; }}
    
    //Helper class for picking child objects
    private Helper help = new Helper();
    
    void Start()
    {              
        //Prearing main game level map group
        //By default gamehood is deactivated
        gameHood.SetActive(false);

        //Main paltform group
        levelMap = new GameObject("LevelMap"); 
        //Assigning default pos
        levelMap.transform.position = Vector3.zero;   
        
    }

    // Update is called once per frame
    void Update()
    {
        if (createLevel)
        {
            //Generating the required level map ground
            if (generateGround)
            {           
                //Ground generation
                //Creating ground group
                groundGrp = new GameObject("Ground");
                groundGrp.transform.position = Vector3.zero;
                
                //Ground platform building
                for (float posZIndex = 0f; posZIndex > -(pHeight * tOffset); posZIndex -= tOffset)
                {
                        var cList = new List<GameObject>();
                        for(float posXIndex = 0f; posXIndex < (pWidth * tOffset); posXIndex += tOffset)
                        {                     
                            var scube = Instantiate(pCube, new Vector3(posXIndex, 0f, posZIndex), pCube.transform.rotation);
                            scube.transform.parent = groundGrp.transform;
                            
                            cList.Add(scube);
                        }
                        sCubes.Add(cList);
                    //  if (sCubes[0].Count >= pWidth) print("PRow created!");
                                    
                }
                readyToBuildChallenge = true;
                groundGrp.transform.parent = levelMap.transform;   

                //For centering of platform rect in 0,0,0
                var newWidth = (pCube.transform.localScale.x * pWidth) / 2;
                var newHeight = (pCube.transform.localScale.x * pHeight) / 2;
                levelMap.transform.position = new Vector3(-(newWidth - (newWidth / 4)), 0f, (newHeight - (newHeight / 4)));

                //Deactivating ground generation mode because it is in update            
                generateGround = false;                

               
            }
                    //Build platform main boundary wall
            if (generateWalls && sCubes.Count > 0)
            {      
                boundWalls = new GameObject("BoundWalls");
                boundWalls.transform.position = Vector3.zero;     
                //New wall spawn logic
                //Vertical wall generation logic
                int indexOffset = 1;
                for (int lIndex = sCubes.Count / 2 - 1; lIndex >= 0; lIndex--)
                {
                    var spawnedWall1 = Instantiate(wallCube, 
                                                //wall position
                                                sCubes[lIndex][0].transform.position + 
                                                //wall pos offset
                                                new Vector3((-pCube.transform.localScale.x / 2) - 5f, //x
                                                wallHeight / 2, //y
                                                0f), //z
                                                wallCube.transform.rotation);
                    spawnedWall1.transform.parent = boundWalls.transform;

                    var spawnedWall2 = Instantiate(wallCube, 
                                                //wall position
                                                sCubes[lIndex][sCubes[lIndex].Count - 1].transform.position + 
                                                //wall pos offset
                                                new Vector3(pCube.transform.localScale.x / 2 + 5f, //x
                                                wallHeight / 2, //y
                                                0f), //z
                                                wallCube.transform.rotation);
                    spawnedWall2.transform.parent = boundWalls.transform;                               
                    
                    var spawnedWall3 = Instantiate(wallCube, 
                                                //wall position
                                                sCubes[lIndex + indexOffset][0].transform.position +
                                                //wall pos offset
                                                new Vector3((-pCube.transform.localScale.x / 2) - 5f, //x
                                                wallHeight / 2, //y
                                                0f), //z
                                                wallCube.transform.rotation);
                    spawnedWall3.transform.parent = boundWalls.transform;

                    var spawnedWall4 = Instantiate(wallCube, 
                                                //wall position
                                                sCubes[lIndex + indexOffset][sCubes[lIndex + indexOffset].Count - 1].transform.position +
                                                //wall pos offset
                                                new Vector3(pCube.transform.localScale.x / 2 + 5f, //x
                                                wallHeight / 2, //y
                                                0f), //z
                                                wallCube.transform.rotation);
                    spawnedWall4.transform.parent = boundWalls.transform;

                    indexOffset += 2;
                }
                //Horizontal wall spawning
                indexOffset = 1;
                for (int lIndex = sCubes[0].Count / 2 - 1; lIndex >= 0; lIndex--)
                {
                    var spawnedWall1 = Instantiate(wallCube, 
                                                sCubes[0][lIndex].transform.position +
                                                new Vector3(0f, wallHeight / 2, (pCube.transform.localScale.x / 2) + 5f), 
                                                Quaternion.Euler(new Vector3(0f, 90f, 0f)));
                    spawnedWall1.transform.parent = boundWalls.transform;

                    var spawnedWall2 = Instantiate(wallCube, 
                                                sCubes[0][lIndex + indexOffset].transform.position + 
                                                new Vector3(0f, wallHeight / 2, (pCube.transform.localScale.x / 2) + 5f), 
                                                Quaternion.Euler(new Vector3(0f, 90f, 0f)));
                    spawnedWall2.transform.parent = boundWalls.transform;
                    
                    var spawnedWall3 = Instantiate(wallCube, 
                                                sCubes[sCubes.Count - 1][lIndex].transform.position + 
                                                new Vector3(0f, wallHeight / 2, -((pCube.transform.localScale.x / 2) + 5f)), 
                                                Quaternion.Euler(new Vector3(0f, 90f, 0f)));
                    spawnedWall3.transform.parent = boundWalls.transform;

                    var spawnedWall4 = Instantiate(wallCube, 
                                                sCubes[sCubes.Count - 1][lIndex + indexOffset].transform.position + 
                                                new Vector3(0f, wallHeight / 2, -((pCube.transform.localScale.x / 2) + 5f)), 
                                                Quaternion.Euler(new Vector3(0f, 90f, 0f)));
                    spawnedWall4.transform.parent = boundWalls.transform;
                    
                    indexOffset += 2;
                }
                //Regulating the scale
                boundWalls.transform.localScale = new Vector3(
                                                                boundWalls.transform.localScale.x,
                                                                wallHeight,
                                                                boundWalls.transform.localScale.z);
                //Parenting to main map group
                boundWalls.transform.parent = levelMap.transform;

                generateWalls = false;
                
            }

            //Creating paltform wall obstacles in cross pattern
            if (createAreaWalls && sCubes.Count > 0)
            {              
               
                wallAreaGrp = new GameObject("AreaWalls");
                wallAreaGrp.transform.position = Vector3.zero;
                bool rotateWall = false;
                int rIndex = 1;
                int cIndex = 1;
                float zOffset = 0f;
                //Are wall generation
                for (int sCubeIndex = pHeight / 2 - 1; sCubeIndex >= 0; sCubeIndex--)
                {
                    //Horizontal wall generation
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
                    //Horizontal wall generation
                    var leftTransform = sCubes[sCubeIndex][pWidth / 2 - 1].transform;
                    var rightTransform = sCubes[sCubeIndex + rIndex][pWidth / 2 - 1].transform;
                    var spawnWall1 = Instantiate(areaWall, leftTransform.position + new Vector3(pCube.transform.localScale.x / 2, 0f, zOffset), leftTransform.rotation);
                    var spawnWall2 = Instantiate(areaWall, rightTransform.position + new Vector3(pCube.transform.localScale.x / 2, 0f, -zOffset), rightTransform.rotation);
                    spawnWall1.transform.parent = wallAreaGrp.transform;
                    spawnWall2.transform.parent = wallAreaGrp.transform;
                    rIndex += 2;
                }            

                //Parenting to main map group
                wallAreaGrp.transform.parent = levelMap.transform;
                
                //Deactivating area wall build
                createAreaWalls = false;

                
            }
            //Creating platform holes
            if (createHoles && sCubes.Count > 0)
            {     
                //Hole group creation     
                var hList = new List<GameObject>();
                holeGrp = new GameObject("Platform holes");
                holeGrp.transform.position = Vector3.zero;

                //Picking near center indexes
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
                            
                            //Switching lanes in next iteration
                            if (holeLaneSwitch)
                            {                              
                                pLane = pFirstLane;
                            }
                            else
                            {
                                pLane = pEndLane;
                            }
                            //For picking random indexes for spawning holes
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
                        
                            //Map hole spawning
                            var sPosLane = pLane[spawnIndexes[randPosIndex]];//hole spawn point                            
                            var spawnedHole = Instantiate(holeCube, sPosLane.transform.position, sPosLane.transform.rotation);
                            holes.Add(spawnedHole);
                            spawnedHole.transform.parent = holeGrp.transform;

                            //Removing existing platform model so it could not overlap
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
                
                //Closing hole generation mode
                createHoles = false;
               
            }

            //Standard mound generation
            if(createMounds && sCubes.Count > 0)
            {    
                //moundCube
                moundGrp = new GameObject("Mounds");
                moundGrp.transform.position = Vector3.zero;

                //Picking near edge platform lanes
                var leftLane = sCubes[(((sCubes.Count - 1) / 2) - 1)];
                var rightLane = sCubes[sCubes.Count - ((sCubes.Count - 1) / 2)];

                //Indexes for random spawning
                var spawnIndexes = new List<int>();
                spawnIndexes.Add(((leftLane.Count - 1) / 2) - 1);
                spawnIndexes.Add(leftLane.Count - ((leftLane.Count - 1) / 2));
                
                for(int mIndex = 0; mIndex < moundCount; mIndex++)
                {
                    moundLaneSwitch = !moundLaneSwitch;
                    List<GameObject> pLane = new List<GameObject>(); 
                    List<GameObject> nPLane = new List<GameObject>(); 
                    var mRotAngle = Vector3.zero; //Rotation angle on Yaxis

                    //For switching in next iteration and rotating mound
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
                    //For picking index position to be random
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
                    var mNPos = nPLane[spawnIndexes[randPosIndex]].transform.position + new Vector3(0f, cPScale.y, 0f);
                    
                    //Adding building navmesh and placing loot boxes on them
                    var spawnedMound = Instantiate(moundCube, mPos, Quaternion.Euler(mRotAngle));
                    mounds.Add(spawnedMound);

                    //For spawning not walkable mounds
                    var spawnedMoundN = Instantiate(moundNormal, mNPos, Quaternion.Euler(mRotAngle));
                    
                    //Parent to required sub group
                    spawnedMound.transform.parent = moundGrp.transform;
                    spawnedMoundN.transform.parent = moundGrp.transform;
                }

                moundGrp.transform.parent = levelMap.transform;

                //Closing mound generation mode
                createMounds = false;
               
            }

            //Energon deposit spawning
            if (spawnEDeposits && sCubes.Count > 0)
            {     
                //createPart(5, delayTime);      
                energonGrp = new GameObject("EnergonDeposits");
                energonGrp.transform.position = Vector3.zero;

                //Picking lanes to spawn simalarly like mounds
                var rightLane = sCubes[((sCubes.Count - 1) / 2) - 1];
                var leftLane = sCubes[(sCubes.Count - 1) / 2];
                var offsetT = pCube.transform.localScale.x / 4;


                var endPosX = rightLane[(rightLane.Count / 2)].transform.position.x - 2 * pCube.transform.localScale.x;           
                
                //Right lane spawning
                //Spawning in the right region energon deposit                
                for(float xPos = rightLane[0].transform.position.x; xPos < endPosX; xPos += offsetT)
                {        
                   // print("Spawning right lane energon deposits");
                    var ePos = new Vector3(xPos, pCube.transform.localScale.y + 2.5f, rightLane[0].transform.position.z - 2 * offsetT);       
                    var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                    spawnedDeposit.transform.parent = energonGrp.transform;
                }

                //Spawning in the left region energon deposits
                endPosX = rightLane[(rightLane.Count - 1)].transform.position.x + pCube.transform.localScale.x;           
                for(float xPos = rightLane[rightLane.Count - 1].transform.position.x - pCube.transform.localScale.x; 
                          xPos < endPosX - 2 * offsetT; 
                          xPos += offsetT)
                {     
                    //print("Spawning right lane energon deposits");   
                    var ePos = new Vector3(xPos, pCube.transform.localScale.y + 2.5f, rightLane[0].transform.position.z - 2 * offsetT);       
                    var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                    spawnedDeposit.transform.parent = energonGrp.transform;
                }
                //--------------------------------
                //Left lane spawning
                endPosX = leftLane[(leftLane.Count / 2)].transform.position.x - 2 * pCube.transform.localScale.x;           
                //endPosX works with both lanes because platform is a square
                for(float xPos = leftLane[0].transform.position.x; xPos < endPosX; xPos += offsetT)
                {     
                    //print("Spawning left lane energon deposits");
                    var ePos = new Vector3(xPos, pCube.transform.localScale.y + 2.5f, leftLane[(leftLane.Count / 2 - 1)].transform.position.z - 10 * offsetT);       
                    var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                    spawnedDeposit.transform.parent = energonGrp.transform;
                }

                //Spawning in the left region energon deposits
                endPosX = leftLane[(leftLane.Count - 1)].transform.position.x - 2 * pCube.transform.localScale.x;           
                for(float xPos = leftLane[leftLane.Count - 2].transform.position.x; xPos >= endPosX; xPos -= offsetT)
                {    
                    //print("Spawning left lane energon deposits");    
                    var ePos = new Vector3(xPos, pCube.transform.localScale.y + 2.5f, leftLane[(leftLane.Count / 2 - 1)].transform.position.z - 10 * offsetT);       
                    var spawnedDeposit = Instantiate(energonDeposit, ePos, energonDeposit.transform.rotation);               
                    spawnedDeposit.transform.parent = energonGrp.transform;
                }
                energonGrp.transform.parent = levelMap.transform;
                
                //Closing deposit spawn mode
                spawnEDeposits = false;
               
            }

            //Spawning main level bases and required game hood
            if (spawnGameBases && sCubes.Count > 0)
            {                          
                // //Activating the game hood
                gameHood.SetActive(true);
                if (!isTimeUnlimited)
                    levelSessionMgr.SetActive(true);
                gSpawnMBaseGrp = new GameObject("Game_MBases");
                gSpawnMBaseGrp.transform.position = Vector3.zero;

                //Picking edge lanes to spawn
                var firstLane = sCubes[0];
                var endLane = sCubes[sCubes.Count - 1];
                var spawnPoints = new List<Transform>();
                
                //Calcualting edge indexes to spawn bases
                spawnPoints.Add(firstLane[0].transform);
                spawnPoints.Add(firstLane[firstLane.Count - 1].transform); //2 cuz for not looking at the attached hole object
                spawnPoints.Add(endLane[0].transform);
                spawnPoints.Add(endLane[firstLane.Count - 1].transform);

                //For offseting on Y axis
                var yPosOffset = pCube.transform.localScale.y + 2f;

                //SpawnPlayerBase
                var sPos = spawnPoints[Random.Range(0, spawnPoints.Count)];            
                var spawnPlayerBase = Instantiate(sPlayerBase, sPos.position + new Vector3(0f, yPosOffset, 0f), sPlayerBase.transform.rotation);
                //transporting main camera to playerbase view
                mainCamera.transform.position = new Vector3(spawnPlayerBase.transform.position.x, mainCamera.transform.position.y, spawnPlayerBase.transform.position.z);
                
                spawnPoints.Remove(sPos);
                spawnPlayerBase.transform.parent = gSpawnMBaseGrp.transform;
                gSpawnMBaseGrp.transform.parent = levelMap.transform;

                //SpawnBorgBase
                sPos = spawnPoints[Random.Range(0, spawnPoints.Count)];
                var spawnBorgBase = Instantiate(sBorgBase, sPos.position + new Vector3(0f, yPosOffset, 0f), sBorgBase.transform.rotation);
                spawnPoints.Remove(sPos);
                spawnBorgBase.transform.parent = gSpawnMBaseGrp.transform;
                gSpawnMBaseGrp.transform.parent = levelMap.transform;   

                //Closing base spawning mode
                spawnGameBases = false;               
            }
            //Loot boxes generation mode
            if (spawnLootBoxes && sCubes.Count > 0)
            {
                lootBoxGrp = new GameObject("LootBoxes");
                lootBoxGrp.transform.position = Vector3.zero;

                //Spawning logic on walkable mounds
                foreach(var m in mounds)
                {
                    var spawnedLootBox = Instantiate(lootBox,
                                                   m.transform.position + 
                                                   new Vector3(0f, 10f, 0f), 
                                                   lootBox.transform.rotation);
                    //Generating random loot box types 0 - 1
                    var boxType = Random.Range(0, 2);
                    //Cehcking if old type number exists or not
                    if (oldBoxType == boxType)
                    {
                        //For picking boxtypes at random
                        while(oldBoxType == boxType)
                        {
                            boxType = Random.Range(0, 2);

                            if (oldBoxType != boxType) 
                            {
                                oldBoxType = boxType;
                                break;
                            }
                        }
                    }
                    else
                    {
                        oldBoxType = boxType;
                    }
                    //Setting randomly picked box type
                    spawnedLootBox.GetComponent<LootBox>().BoxType = boxType;

                    //Placing loot box to sub group
                    spawnedLootBox.transform.parent = lootBoxGrp.transform;
                }

                //Spawning lootboxes on holes
                foreach(var h in holes)
                {
                    var spawnedLootBox = Instantiate(lootBox,
                                                   h.transform.position + 
                                                   new Vector3(0f, 5f, 0f), 
                                                   lootBox.transform.rotation);

                    //Generating random loot box types 0 - 1
                    var boxType = Random.Range(0, 2);
                    //Cehcking if old type number exists or not
                    if (oldBoxType == boxType)
                    {
                        while(oldBoxType == boxType)
                        {
                            boxType = Random.Range(0, 2);

                            if (oldBoxType != boxType) 
                            {
                                oldBoxType = boxType;
                                break;
                            }
                        }
                    }
                    else
                    {
                        oldBoxType = boxType;
                    }

                    spawnedLootBox.GetComponent<LootBox>().BoxType = boxType;
                    spawnedLootBox.transform.parent = lootBoxGrp.transform;
                }
                //Parenting the created loot box group
                lootBoxGrp.transform.parent = levelMap.transform;
                spawnLootBoxes = false;
            }
            //NOTE: Generate NavMesh at the end
           if (generateNavMesh && sCubes.Count > 0)
           {   
               gameObject.GetComponent<NavMeshSurface>().BuildNavMesh();               

               //Closing navmesh generation mode  
               generateNavMesh = false;               
            }
            //Closing auto level system
            createLevel = false;
        }
        
    }
  
}
