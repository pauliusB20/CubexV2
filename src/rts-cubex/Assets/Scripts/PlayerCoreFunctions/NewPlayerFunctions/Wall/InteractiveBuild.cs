using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InteractiveBuild : MonoBehaviour
{
    [Header("Configuration parameters")]
    [Header("GUI cnf.")]
    [SerializeField] GameObject errorWindow;
    [SerializeField] Text errorMsg;
    [SerializeField] Button buildButton;
    [Header("System cnf.")]
    [SerializeField] GameObject buildingIcon;
    [SerializeField] GameObject buildingArea; 
    [SerializeField] GameObject buildArea; // needed to create animated pop ups
    [SerializeField] float rotationSpeed = 60f;
    private GameObject sBuildingIcon; //appeared in game
    private Base playerBase;
    private bool openBMode = false;
    Ray rayMouse;
    RaycastHit mouseHit;
    private bool isRotated = false;
    private int energonPriceToBuild = 0;
    private int creditsPriceToBuild = 0;
    private bool dragBuild = false;
    private bool startToBuild = false; //For shift
    private Vector3 oldBuildIconPos;
    private GameObject iniBuilding;
    private Color originalIconColor;
    [SerializeField] int allBEnergonPrice = 0;
    [SerializeField] int allBCreditsPrice = 0;
    public bool OpenBMode { set {openBMode = value; } get {return openBMode;}}
    public int AllBEnergonPrice { set {allBEnergonPrice = value;} get {return allBEnergonPrice;}}
    public int AllBCreditsPrice { set {allBCreditsPrice = value;} get {return allBCreditsPrice;}}
    [Header("Debug data")]
    [SerializeField] List<GameObject> shiftSpawnedBuilds;
    private Helper helperTools = new Helper();
    [SerializeField] Text availableWallsAmount;
    [SerializeField] bool lockBtn = false;
    [SerializeField] Text btnText;
    public void interactiveBuildMode() //Button method to open build mode
    {
        if (sBuildingIcon)
        {
            openBMode = true;
            sBuildingIcon.SetActive(true);
            buildingArea.SetActive(true);
            buildButton.interactable = false;
        }
        else
        {
            print("ERROR: No wall icon spawned!");
        }
    }
    void Start()
    {
        shiftSpawnedBuilds = new List<GameObject>();
        playerBase = GetComponent<Base>();
        if (buildingIcon)
        {
            sBuildingIcon = Instantiate(buildingIcon, new Vector3(transform.position.x, buildArea.transform.position.y, transform.position.z), buildingIcon.transform.rotation);
            
            if (sBuildingIcon)
            {
                sBuildingIcon.SetActive(false);
                var sBuildingModel = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                if (sBuildingModel)
                {
                    sBuildingModel.GetComponent<Collider>().isTrigger = true;
                    originalIconColor = sBuildingModel.GetComponent<MeshRenderer>().material.color;
                    energonPriceToBuild = sBuildingModel.GetComponent<WallBuildCheck>().EnergonPrice;
                    creditsPriceToBuild = sBuildingModel.GetComponent<WallBuildCheck>().CreditsPrice;
                }
            }
        }
        availableWallsAmount.text = playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel + " / " + playerBase.GetComponent<setFidexAmountOfStructures>().getMaxPlayerWallsAmountInLevel;
    }

    // Update is called once per frame
    void Update()
    {
        availableWallsAmount.text = playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel + " / " + playerBase.GetComponent<setFidexAmountOfStructures>().getMaxPlayerWallsAmountInLevel;
        if (playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel < playerBase.GetComponent<setFidexAmountOfStructures>().getMaxPlayerWallsAmountInLevel){
            buildButton.interactable = true;
            lockBtn = false;
            btnText.text = "Create Wall \n" + "(" + energonPriceToBuild + " energon &" + creditsPriceToBuild + " credits)";
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            openBMode = false;           
            dragBuild = false;
            cleanShiftBuildings();
        }
        if (openBMode)
        {           
           // BuildingIcon tracking
            var mousePosition = Input.mousePosition;
            rayMouse = Camera.main.ScreenPointToRay(mousePosition);
            if(Physics.Raycast(rayMouse, out mouseHit, Mathf.Infinity))
            {
                //print(mouseHit.transform.gameObject.transform.tag);
                
                if (mouseHit.collider.tag == buildingArea.tag)
                {                    
                    // Main drag build snapping logic 
                    if (dragBuild)
                    {
                        // Snapping transformation on required axis
                        // Snapping on X axis
                        var currentPos = mouseHit.point;
                        if (isRotated)
                        {                     
                            sBuildingIcon.transform.position = new Vector3(oldBuildIconPos.x, buildArea.transform.position.y, currentPos.z);
                        }
                        else //Snapping Z axis
                        {
                            sBuildingIcon.transform.position = new Vector3(currentPos.x, buildArea.transform.position.y, oldBuildIconPos.z);
                        }
                        // Add initial building to sBuildingIcon
                        if (!iniBuilding)
                        {
                            iniBuilding = Instantiate(sBuildingIcon, sBuildingIcon.transform.position, sBuildingIcon.transform.rotation);
                            iniBuilding.tag = "WallHolo";
                            iniBuilding.name = iniBuilding.name + "_s";                            
                            var rayBuilder = helperTools.getChildGameObjectByName(iniBuilding, "Wall_1");
                            rayBuilder.GetComponent<Collider>().isTrigger = false;
                            rayBuilder.GetComponent<WallBuildCheck>().IsTemp = true;
                            var rayComp = rayBuilder.GetComponent<BuildRayField>();
                            var sIcon = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                            var prepRay = sIcon.GetComponent<BuildRayField>(); 
                            prepRay.SBuilding = iniBuilding;
                            prepRay.makeStartCopy();
                            prepRay.IsRotated = isRotated;
                            //prepRay.SpawnedShiftBuilds = new List<GameObject>();
                            prepRay.SpawnedShiftBuilds.Add(iniBuilding);
                            rayComp.IsStartBuilding = true;
                            rayComp.CheckBuildings = false;
                            rayBuilder.layer = LayerMask.NameToLayer("HoloTools");

                            AllBEnergonPrice += rayBuilder.GetComponent<WallBuildCheck>().EnergonPrice;
                            AllBCreditsPrice += rayBuilder.GetComponent<WallBuildCheck>().CreditsPrice;
                        }
                    }
                    else
                    {
                        sBuildingIcon.transform.position = new Vector3(mouseHit.point.x, 0.5f, mouseHit.point.z);
                        oldBuildIconPos = sBuildingIcon.transform.position;
                    }
                }
                
            }
           //Check if player hitted the rotate mouse key
            if (Input.GetMouseButtonDown(1) && !dragBuild)
            {
                if (isRotated)
                {
                    sBuildingIcon.transform.eulerAngles = Vector3.Lerp(sBuildingIcon.transform.rotation.eulerAngles, new Vector3(0f, 0f, 0f), Time.deltaTime * rotationSpeed);
                    isRotated = false;
                    return;
                }
                sBuildingIcon.transform.eulerAngles = Vector3.Lerp(sBuildingIcon.transform.rotation.eulerAngles, new Vector3(0f, 90f, 0f), Time.deltaTime * rotationSpeed);
                isRotated = !isRotated;
            }
               // DragBuild mode start logic
                if (Input.GetKeyDown(KeyCode.LeftShift))
                {
                    dragBuild = true;
                    //buildButton.interactable = false;
                    var bIconModel = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                    if (bIconModel)
                    {
                        var bCol = bIconModel.GetComponent<BuildRayField>();
                        bCol.CheckBuildings = true;
                        if (bCol.CheckBuildings)
                        {
                            bIconModel.GetComponent<MeshRenderer>().material.color = new Color(0f, 255f, 0f, 0.25f);
                            print("Started drag build!");
                        }
                    }
                }
            //Shift build construct logic  
            if (Input.GetMouseButtonDown(0) && dragBuild)
            {
                startToBuild = true;
            }
            if (dragBuild)
            {
                //print("Shift process started!!");
                if (Input.GetMouseButtonDown(0) && dragBuild)
                {
                    // Counting remaing required funds                    
                    var sBuildIconModel = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                    var rayBuilder = sBuildIconModel.GetComponent<BuildRayField>();

                    AllBEnergonPrice += rayBuilder.AllBEnergonPrice;
                    AllBCreditsPrice += rayBuilder.AllBCreditsPrice;

                    // Check if enough funds are available and no collisions are in front
                    var bIModel = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                    var bIChecker = bIModel.GetComponent<WallBuildCheck>();
                    if ((playerBase.getEnergonAmount() < AllBEnergonPrice || 
                         playerBase.getCreditsAmount() < AllBCreditsPrice) ||
                         bIChecker.SpaceOccupied)
                    {
                        AllBEnergonPrice = 0;
                        AllBCreditsPrice = 0;

                        rayBuilder.AllBEnergonPrice = 0;
                        rayBuilder.AllBCreditsPrice = 0;
                        rayBuilder.SBuilding = null;
                        rayBuilder.CheckBuildings = false;
                        rayBuilder.SpawnedShiftBuilds = new List<GameObject>(); 
                        cleanShiftBuildings();
                        playerBase.GetComponent<LocalPanelManager>().deactivatePanels();
                        errorWindow.SetActive(true);                        
                        errorMsg.text = (bIChecker.SpaceOccupied ? "Obstacle nearby! can't construct buildings!" : "Not enought funds to build these buildings!");
                        openBMode = false; 
                        dragBuild = false;

                        print("ERROR: " + (bIChecker.SpaceOccupied ? "Obstacle nearby! can't construct buildings!" : "Not enought funds to build these buildings!"));
                        return;
                    }

                    var bList = rayBuilder.SpawnedShiftBuilds;
                    if (bList.Count > 0)
                    {
                        print("Building each wall!");
                        foreach (var b in bList)
                        {
                            if (b != null)
                            {
                                var bModel = helperTools.getChildGameObjectByName(b, "Wall_1");
                                var bCheck = bModel.GetComponent<WallBuildCheck>();
                                if (bModel && bCheck.IsTemp)
                                {
                                    bCheck.IsBuilt = true;
                                    bCheck.IsTemp = false;
                                    b.tag = "_PlayerWall";
                                    bModel.tag = "PlayerWall";
                                    bModel.GetComponent<Collider>().isTrigger = true;
                                    bModel.layer = LayerMask.NameToLayer("PlayerBase");
                                }
                            }
                        }  
                        rayBuilder.SpawnedShiftBuilds = new List<GameObject>();  
                        playerBase.GetComponent<createAnimatedPopUp>().createDecreaseCreditsPopUp(AllBCreditsPrice); // creating animated pop ups
                        playerBase.GetComponent<createAnimatedPopUp>().createDecreaseEnergonPopUp(AllBEnergonPrice); // creating animated  pop ups
                        playerBase.setEnergonAmount(playerBase.getEnergonAmount() - AllBEnergonPrice);
                        playerBase.setCreditsAmount(playerBase.getCreditsAmount() - AllBCreditsPrice);                     
                    }
                    // Stopping shift build after construction
                    dragBuild = false;
                    openBMode = false;
                    iniBuilding = null;                    
                    var bIconModel = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                    if (bIconModel)
                    {
                        var bCol = bIconModel.GetComponent<BuildRayField>();
                        bCol.CheckBuildings = false;
                        bCol.SBuilding = null;
                        if (!bCol.CheckBuildings && !bCol.SBuilding)
                        {
                            bIconModel.GetComponent<MeshRenderer>().material.color = originalIconColor;
                            print("Stoped drag build! After built buildings");
                        }
                    }
                    startToBuild = false;
                    
                }
                 // Stop drag build 
                if (Input.GetKeyUp(KeyCode.LeftShift))
                {
                    dragBuild = false;
                    iniBuilding = null;
                    var bIconModel = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                    if (bIconModel)
                    {
                        var bCol = bIconModel.GetComponent<BuildRayField>();
                        bCol.CheckBuildings = false;
                        bCol.SBuilding = null;
                        if (!bCol.CheckBuildings && !bCol.SBuilding)
                        {
                            bIconModel.GetComponent<MeshRenderer>().material.color = originalIconColor;
                            print("Stoped drag build!");
                        }
                    }
                    cleanShiftBuildings();
                }   
            }
            else
            {
                // Checking if player hitten the mouse build button to build one building
                if (Input.GetMouseButtonDown(0) && !dragBuild)
                {
                    if (sBuildingIcon)
                    {
                        var bModel = helperTools.getChildGameObjectByName(sBuildingIcon, "Wall_1");
                        if (bModel)
                        {
                            if (playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel >= playerBase.GetComponent<setFidexAmountOfStructures>().getMaxPlayerWallsAmountInLevel){
                                availableWallsAmount.text = playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel + " / " + playerBase.GetComponent<setFidexAmountOfStructures>().getMaxPlayerWallsAmountInLevel;
                                print("ERROR: Maximum wall amount limit reached!");
                                playerBase.GetComponent<LocalPanelManager>().deactivatePanels();
                                errorWindow.SetActive(true);
                                errorMsg.text = "Maximum wall amount limit reached!";
                                openBMode = false; 
                                lockBtn = true;
                                return;
                            }
                            var wallCheck = bModel.GetComponent<WallBuildCheck>();
                            if (!wallCheck.SpaceOccupied)
                            {
                                if (playerBase.getEnergonAmount() >= energonPriceToBuild && 
                                    playerBase.getCreditsAmount() >= creditsPriceToBuild)
                                {
                                    var sNewBuilding = Instantiate(sBuildingIcon, sBuildingIcon.transform.position, sBuildingIcon.transform.rotation);
                                    sNewBuilding.tag = "_PlayerWall";
                                    var buildModel = helperTools.getChildGameObjectByName(sNewBuilding, "Wall_1");
                                    // if (buildModel)
                                    // {
                                        var bData = buildModel.GetComponent<WallBuildCheck>();
                                        bData.IsBuilt = true;
                                        //print(bData.IsBuilt ? "Built" : "Not built");
                                        buildModel.GetComponent<Collider>().isTrigger = false;
                                        //buildModel.GetComponent<MeshRenderer>().material.color = bData.WallBuiltMat.color;
                                        buildModel.tag = "PlayerWall";
                                        buildModel.layer = LayerMask.NameToLayer("PlayerBase");
                                        //bData.IsBuilt = true;
                                        //var buildArea = GameObject.FindGameObjectWithTag("BuildArea");
                                        playerBase.GetComponent<createAnimatedPopUp>().createDecreaseCreditsPopUp(creditsPriceToBuild);
                                        playerBase.GetComponent<createAnimatedPopUp>().createDecreaseEnergonPopUp(energonPriceToBuild);
                                        playerBase.setEnergonAmount(playerBase.getEnergonAmount() - energonPriceToBuild);
                                        playerBase.setCreditsAmount(playerBase.getCreditsAmount() - creditsPriceToBuild);
                                        playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel = playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel + 1;
                                        availableWallsAmount.text = playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel + " / " + playerBase.GetComponent<setFidexAmountOfStructures>().getMaxPlayerWallsAmountInLevel;
                                        if (playerBase.GetComponent<setFidexAmountOfStructures>().changePlayerWallsAmountInLevel >= playerBase.GetComponent<setFidexAmountOfStructures>().getMaxPlayerWallsAmountInLevel){
                                            btnText.text = "Create Wall\n" + "Max amount reached";
                                            lockBtn = true;
                                        }
                                        openBMode = false;
                                    // }                            
                                    // else
                                    // {
                                    //     print("ERROR building not spawned!");
                                    // }
                                }
                                else
                                {
                                    print("Error: Not enough funds to build!");
                                    playerBase.GetComponent<LocalPanelManager>().deactivatePanels();
                                    errorWindow.SetActive(true);
                                    errorMsg.text = "Not enough funds to build a wall here!";
                                    openBMode = false;  
                                   // buildButton.interactable = true;
                                }
                            }
                            else
                            {
                                print("ERROR: Can't place building here! Space occupied!");
                                playerBase.GetComponent<LocalPanelManager>().deactivatePanels();
                                errorWindow.SetActive(true);
                                errorMsg.text = "Can't place building here! Space occupied!";
                                openBMode = false; 
                                //buildButton.interactable = true; 
                            }
                        }
                        else
                        {
                            Debug.LogError("Error! No building model present!");
                        }
                    }
                    else
                    {
                        Debug.LogError("ERROR: Build Icon not present!");
                        return;
                    }
                }
            }                 
        }
        else
        {
            sBuildingIcon.SetActive(false);
            buildingArea.SetActive(false);
            if(!lockBtn){
                buildButton.interactable = true;
            }
            else{
                buildButton.interactable = false;
            }
            return;
        }
    }
    private void cleanShiftBuildings() //Method required to delete temp buildings
    {
        var holoBlds = GameObject.FindGameObjectsWithTag("WallHolo");
        foreach(var b in holoBlds)
            Destroy(b);
        
        print(holoBlds.Length <= 0 ? "Cleaned all shift buildings" : "ERROR some shifts remain!");
    }
    
    private bool isGListNonNull(List<GameObject> l)
    {
        int a = 0;
        foreach (var i in l)
        {
            if (i != null)
                a++;
        }
        return a > 0;
    }
}
