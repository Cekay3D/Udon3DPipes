using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace Cekay.Pipes
{
    public class Pipe : UdonSharpBehaviour
    {
        [SerializeField] State GlobalPipe;

        [SerializeField] Material[] Materials;

        [SerializeField] GameObject Straight;
        [SerializeField] GameObject Bend;
        [SerializeField] GameObject BendBall;
        [SerializeField] GameObject Cap;
        [SerializeField] GameObject End;
        [SerializeField] GameObject ParentObject;
        [SerializeField] Transform Parent;

        private DataList DirectionList = new DataList()
        {
        (DataToken)0,
        (DataToken)1,
        (DataToken)2,
        (DataToken)3,
        (DataToken)4,
        (DataToken)5
        };

        private Material ChosenColor;

        private GameObject LastObject;

        private string LastPiece = "cap";

        public bool SetFinalPiece = false;
        public bool Completed = false;

        public int Count = 0;
        public int MaxCount = 150;
        private int CurrentDirection = 0;
        private int LastDirection = 0;

        private Vector3 Index = new Vector3(0, 0, 0);
        private Vector3 IndexNew = new Vector3(0, 0, 0);
        private Vector3 IndexTemp = new Vector3(0, 0, 0);
        private Vector3 GoLeft = new Vector3(0, 0, -90);
        private Vector3 GoRight = new Vector3(0, 0, 90);
        private Vector3 GoDown = new Vector3(0, 0, 0);
        private Vector3 GoUp = new Vector3(0, 0, 180);
        private Vector3 GoBackward = new Vector3(90, 0, 0);
        private Vector3 GoForward = new Vector3(-90, 0, 0);
        private Vector3 CurrentDirectionVector = new Vector3(0, 0, 0);
        private Vector3 CurrentRotation = new Vector3(0, 0, 0);
        private Vector3 CurrentBendRotation = new Vector3(0, 0, 0);

        public void GetIndex()
        {
            MaxCount = Random.Range(50, 600);

            MeshRenderer straightMesh = Straight.GetComponent<MeshRenderer>();
            MeshRenderer bendMesh = Bend.GetComponent<MeshRenderer>();
            MeshRenderer bendBallMesh = BendBall.GetComponent<MeshRenderer>();
            MeshRenderer capMesh = Cap.GetComponent<MeshRenderer>();
            MeshRenderer endMesh = End.GetComponent<MeshRenderer>();

            int randomColor = (Random.Range(0, Materials.Length));
            ChosenColor = Materials[randomColor];
            straightMesh.material = ChosenColor;
            bendMesh.material = ChosenColor;
            bendBallMesh.material = ChosenColor;
            capMesh.material = ChosenColor;
            endMesh.material = ChosenColor;

            bool takenStart = true;
            while (takenStart)
            {
                IndexTemp = new Vector3(Random.Range(1, GlobalPipe.GridX), Random.Range(1, GlobalPipe.GridY), Random.Range(1, GlobalPipe.GridZ));
                if (!GlobalPipe.ReadList(IndexTemp))
                {
                    takenStart = false;
                    Index = IndexTemp;
                }
            }
            GetNewDirection();
        }

        public void Step()
        {
            string pipePiece = "straight";

            if (Count == 0)
            {
                pipePiece = "cap";
            }

            int shouldChangeDirection = 0;
            if (Count != 0 && Count != MaxCount && !SetFinalPiece)
            {
                shouldChangeDirection = Random.Range(0, 7);
            }

            if (GlobalPipe.ReadList(Index))
            {
                MeshRenderer lastMesh = LastObject.GetComponent<MeshRenderer>();
                Destroy(lastMesh.gameObject);

                pipePiece = "end";
                PlaceObject(pipePiece, (Index - CurrentDirectionVector), Quaternion.Euler(CurrentRotation));

                Completed = true;
            }
            else if (SetFinalPiece == true)
            {
                pipePiece = "end";
                PlaceObject(pipePiece, Index, Quaternion.Euler(CurrentRotation));
                Completed = true;
            }
            else if (((Index.x == GlobalPipe.GridX && CurrentDirection == 1) || (Index.x == 0 && CurrentDirection == 0)) ||
                ((Index.y == GlobalPipe.GridY && CurrentDirection == 3) || (Index.y == 0 && CurrentDirection == 2)) ||
                ((Index.z == GlobalPipe.GridZ && CurrentDirection == 5) || (Index.z == 0 && CurrentDirection == 4)) ||
                GlobalPipe.ReadList(Index + CurrentDirectionVector) || shouldChangeDirection == 1)
            {
                GetNewDirection();
                switch (GlobalPipe.BendType)
                {
                    case "elbow":
                        pipePiece = "bend";
                        break;
                    case "ball":
                        pipePiece = "ball";
                        break;
                    case "mixed":
                        if ((Random.Range(0, 2)) == 0)
                        {
                            pipePiece = "bend";
                        }
                        else
                        {
                            pipePiece = "ball";
                        }
                        break;
                }
                PlaceObject(pipePiece, Index, Quaternion.Euler(CurrentBendRotation));
            }
            else
            {
                PlaceObject(pipePiece, Index, Quaternion.Euler(CurrentRotation));
            }

            if (Count == MaxCount - 1)
            {
                SetFinalPiece = true;
            }

            GlobalPipe.SetList(Index);

            IndexNew = Index;

            switch (CurrentDirection)
            {
                case 0: // Left
                    IndexNew.x--;
                    break;
                case 1: // Right
                    IndexNew.x++;
                    break;
                case 2: // Down
                    IndexNew.y--;
                    break;
                case 3: // Up
                    IndexNew.y++;
                    break;
                case 4: // Backward
                    IndexNew.z--;
                    break;
                case 5: // Forward
                    IndexNew.z++;
                    break;
            }

            Index = IndexNew;

            LastPiece = pipePiece;
        }

        private void PlaceObject(string pipePiece, Vector3 location, Quaternion DesiredRotation)
        {
            switch (pipePiece)
            {
                case "straight":
                    GameObject pieceStraight = GameObject.Instantiate(Straight, location, DesiredRotation, Parent);
                    pieceStraight.name = "Index: " + Index.ToString();
                    Collider sC = pieceStraight.GetComponent<Collider>();
                    sC.enabled = GlobalPipe.IsColliding;
                    LastObject = pieceStraight;
                    break;

                case "bend":
                    GameObject pieceBend = GameObject.Instantiate(Bend, location, Quaternion.identity, Parent);
                    pieceBend.name = "Index: " + Index.ToString();
                    Transform pieceBendTransform = pieceBend.GetComponent<Transform>();
                    pieceBendTransform.localEulerAngles = CurrentBendRotation;
                    Collider beC = pieceBend.GetComponent<Collider>();
                    beC.enabled = GlobalPipe.IsColliding;
                    LastObject = pieceBend;
                    break;

                case "ball":
                    GameObject pieceBall = GameObject.Instantiate(BendBall, location, Quaternion.identity, Parent);
                    Transform pieceBallTransform = pieceBall.GetComponent<Transform>();
                    pieceBallTransform.localEulerAngles = CurrentBendRotation;
                    Collider baC = pieceBall.GetComponent<Collider>();
                    baC.enabled = GlobalPipe.IsColliding;
                    LastObject = pieceBall;
                    break;

                case "cap":
                    GameObject pieceCap = GameObject.Instantiate(Cap, location, DesiredRotation, Parent);
                    pieceCap.name = "Index: " + Index.ToString();
                    Collider cC = pieceCap.GetComponent<Collider>();
                    cC.enabled = GlobalPipe.IsColliding;
                    LastObject = pieceCap;
                    break;

                case "end":
                    GameObject pieceEnd = GameObject.Instantiate(End, location, DesiredRotation, Parent);
                    pieceEnd.name = "Index: " + Index.ToString();
                    Collider eC = pieceEnd.GetComponent<Collider>();
                    eC.enabled = GlobalPipe.IsColliding;
                    LastObject = pieceEnd;
                    break;

            }
        }

        private void GetNewDirection()
        {
            DataList DirectionListCopy = DirectionList.ShallowClone();

            LastDirection = CurrentDirection;
            switch (LastDirection)
            {
                case 0: // Left
                    DirectionListCopy.RemoveAll(1);
                    break;
                case 1: // Right
                    DirectionListCopy.RemoveAll(0);
                    break;
                case 2: // Down
                    DirectionListCopy.RemoveAll(3);
                    break;
                case 3: // Up
                    DirectionListCopy.RemoveAll(2);
                    break;
                case 4: // Backward
                    DirectionListCopy.RemoveAll(5);
                    break;
                case 5: // Forward
                    DirectionListCopy.RemoveAll(4);
                    break;
            }
            DirectionListCopy.RemoveAll(LastDirection);

            if ((Index.x == GlobalPipe.GridX) || GlobalPipe.ReadList(Index + Vector3.right))
            {
                DirectionListCopy.RemoveAll(1);
            }
            if ((Index.x == 0) || GlobalPipe.ReadList(Index + Vector3.left))
            {
                DirectionListCopy.RemoveAll(0);
            }
            if ((Index.y == GlobalPipe.GridY) || GlobalPipe.ReadList(Index + Vector3.up))
            {
                DirectionListCopy.RemoveAll(3);
            }
            if ((Index.y == 0) || GlobalPipe.ReadList(Index + Vector3.down))
            {
                DirectionListCopy.RemoveAll(2);
            }
            if ((Index.z == GlobalPipe.GridZ) || GlobalPipe.ReadList(Index + Vector3.forward))
            {
                DirectionListCopy.RemoveAll(5);
            }
            if ((Index.z == 0) || GlobalPipe.ReadList(Index + Vector3.back))
            {
                DirectionListCopy.RemoveAll(4);
            }

            if (DirectionListCopy.Count == 0)
            {
                SetFinalPiece = true;
            }
            else
            {
                CurrentDirection = DirectionListCopy[(Random.Range(0, DirectionListCopy.Count))].Int;
                GetNewRotation();
            }
        }

        private void GetNewRotation()
        {
            switch (CurrentDirection)
            {
                case 0: // Left
                    CurrentRotation = GoLeft;
                    CurrentDirectionVector = Vector3.left;
                    break;
                case 1: // Right
                    CurrentRotation = GoRight;
                    CurrentDirectionVector = Vector3.right;
                    break;
                case 2: // Down
                    CurrentRotation = GoDown;
                    CurrentDirectionVector = Vector3.down;
                    break;
                case 3: // Up
                    CurrentRotation = GoUp;
                    CurrentDirectionVector = Vector3.up;
                    break;
                case 4: // Backward
                    CurrentRotation = GoBackward;
                    CurrentDirectionVector = Vector3.back;
                    break;
                case 5: // Forward
                    CurrentRotation = GoForward;
                    CurrentDirectionVector = Vector3.forward;
                    break;
            }
            switch (LastDirection)
            {
                case 0: // Left
                    if (CurrentDirection == 2)
                    {
                        CurrentBendRotation = new Vector3(0, 0, 0);
                    }
                    if (CurrentDirection == 3)
                    {
                        CurrentBendRotation = new Vector3(0, 0, 90);
                    }
                    if (CurrentDirection == 4)
                    {
                        CurrentBendRotation = new Vector3(-90, 0, 90);
                    }
                    if (CurrentDirection == 5)
                    {
                        CurrentBendRotation = new Vector3(90, 0, 90);
                    }
                    break;
                case 1: // Right
                    if (CurrentDirection == 2)
                    {
                        CurrentBendRotation = new Vector3(0, 180, 0);
                    }
                    if (CurrentDirection == 3)
                    {
                        CurrentBendRotation = new Vector3(0, 0, 180);
                    }
                    if (CurrentDirection == 4)
                    {
                        CurrentBendRotation = new Vector3(90, 90, 0);
                    }
                    if (CurrentDirection == 5)
                    {
                        CurrentBendRotation = new Vector3(-90, 0, -90);
                    }
                    break;
                case 2: // Down
                    if (CurrentDirection == 0)
                    {
                        CurrentBendRotation = new Vector3(0, 0, 180);
                    }
                    if (CurrentDirection == 1)
                    {
                        CurrentBendRotation = new Vector3(0, 0, 90);
                    }
                    if (CurrentDirection == 4)
                    {
                        CurrentBendRotation = new Vector3(0, 90, 90);
                    }
                    if (CurrentDirection == 5)
                    {
                        CurrentBendRotation = new Vector3(180, -90, 0);
                    }
                    break;
                case 3: // Up
                    if (CurrentDirection == 0)
                    {
                        CurrentBendRotation = new Vector3(0, 180, 0);
                    }
                    if (CurrentDirection == 1)
                    {
                        CurrentBendRotation = new Vector3(0, 0, 0);
                    }
                    if (CurrentDirection == 4)
                    {
                        CurrentBendRotation = new Vector3(0, 90, 0);
                    }
                    if (CurrentDirection == 5)
                    {
                        CurrentBendRotation = new Vector3(0, -90, 0);
                    }
                    break;
                case 4: // Backward
                    if (CurrentDirection == 0)
                    {
                        CurrentBendRotation = new Vector3(-90, 0, -90);
                    }
                    if (CurrentDirection == 1)
                    {
                        CurrentBendRotation = new Vector3(-90, 0, 0);
                    }
                    if (CurrentDirection == 2)
                    {
                        CurrentBendRotation = new Vector3(0, -90, 0);
                    }
                    if (CurrentDirection == 3)
                    {
                        CurrentBendRotation = new Vector3(180, -90, 0);
                    }
                    break;
                case 5: // Forward
                    if (CurrentDirection == 0)
                    {
                        CurrentBendRotation = new Vector3(-90, 180, 0);
                    }
                    if (CurrentDirection == 1)
                    {
                        CurrentBendRotation = new Vector3(90, 0, 0);
                    }
                    if (CurrentDirection == 2)
                    {
                        CurrentBendRotation = new Vector3(0, 90, 0);
                    }
                    if (CurrentDirection == 3)
                    {
                        CurrentBendRotation = new Vector3(180, 90, 0);
                    }
                    break;
            }
        }

        public void DestroyParts()
        {
            MeshRenderer[] partsToDestroy = ParentObject.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer obj in partsToDestroy)
            {
                Destroy(obj.gameObject);
            }
        }

        public void Collide(bool ShouldCollide)
        {
            Collider[] colliders = ParentObject.GetComponentsInChildren<Collider>();

            foreach (Collider c in colliders)
            {
                c.enabled = ShouldCollide;
            }
        }
    }
}