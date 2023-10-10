using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDK3.Data;

namespace Cekay.Pipes
{
    public class State : UdonSharpBehaviour
    {
        [SerializeField] private Pipe One;
        [SerializeField] private Pipe Two;
        [SerializeField] private Pipe Three;
        [SerializeField] private Pipe Four;
        [SerializeField] private Pipe Five;
        [SerializeField] private Pipe Six;
        [SerializeField] private Pipe Seven;

        public Material[] Materials;

        [SerializeField] private Slider SpeedSlider;
        [SerializeField] private Toggle SingleButton;
        [SerializeField] private Toggle MultipleButton;
        [SerializeField] private TextMeshProUGUI CollideText;

        public DataList StateList = new DataList();

        public string BendType = "elbow";

        public bool IsColliding = true;
        private bool IsPaused = false;
        private bool DoSingle = false;
        private bool WakeTwo = false;
        private bool WakeThree = false;
        private bool WakeFour = false;
        private bool WakeFive = false;
        private bool WakeSix = false;
        private bool WakeSeven = false;

        public int InitialPipes = 1;
        public readonly int GridX = 35;
        public readonly int GridY = 20;
        public readonly int GridZ = 35;

        public float IntervalFade = .2f;
        private float FadeLevel = 1.0f;
        private float Interval = 0.1f;
        private float IntervalStored = 0.1f;

        private void Start()
        {
            StateList.Clear();
            StartPipes();
            StepAll();
        }

        public void StartPipes()
        {
            One.GetIndex();
            if (!DoSingle)
            {
                InitialPipes = Random.Range(2, 5);

                if (InitialPipes == 2)
                {
                    Two.GetIndex();
                    WakeTwo = true;
                }
                if (InitialPipes == 3)
                {
                    Two.GetIndex();
                    WakeTwo = true;
                    Three.GetIndex();
                    WakeThree = true;
                }
                if (InitialPipes == 4)
                {
                    Two.GetIndex();
                    WakeTwo = true;
                    Three.GetIndex();
                    WakeThree = true;
                    Four.GetIndex();
                    WakeFour = true;
                }
            }
        }

        public void StepAll()
        {
            if (!IsPaused)
            {
                if (!One.Completed && (One.Count <= One.MaxCount))
                {
                    One.Step();
                    One.Count++;
                }

                if (!DoSingle)
                {
                    if (!Two.Completed && (Two.Count <= Two.MaxCount))
                    {
                        Two.Step();
                        Two.Count++;
                    }

                    if (WakeThree && !Three.Completed && (Three.Count <= Three.MaxCount))
                    {
                        Three.Step();
                        Three.Count++;
                    }
                    else if (!WakeThree)
                    {
                        WakeThree = ShouldWake();
                        if (WakeThree)
                        {
                            Three.GetIndex();
                            WakeThree = true;
                        }
                    }

                    if (WakeFour && !Four.Completed && (Four.Count <= Four.MaxCount))
                    {
                        Four.Step();
                        Four.Count++;
                    }
                    else if (!WakeFour)
                    {
                        WakeFour = ShouldWake();
                        if (WakeFour)
                        {
                            Four.GetIndex();
                            WakeFour = true;
                        }
                    }

                    if (WakeFive && !Five.Completed && (Five.Count <= Five.MaxCount))
                    {
                        Five.Step();
                        Five.Count++;
                    }
                    else if (!WakeFive)
                    {
                        WakeFive = ShouldWake();
                        if (WakeFive)
                        {
                            Five.GetIndex();
                            WakeFive = true;
                        }
                    }

                    if (WakeSix && !Six.Completed && (Six.Count <= Six.MaxCount))
                    {
                        Six.Step();
                        Six.Count++;
                    }
                    else if (!WakeSix)
                    {
                        WakeSix = ShouldWake();
                        if (WakeSix)
                        {
                            Six.GetIndex();
                            WakeSix = true;
                        }
                    }

                    if (WakeSeven && !Seven.Completed && (Seven.Count <= Seven.MaxCount))
                    {
                        Seven.Step();
                        Seven.Count++;
                    }
                    else if (!WakeSeven)
                    {
                        WakeSeven = ShouldWake();
                        if (WakeSeven)
                        {
                            Seven.GetIndex();
                            WakeSeven = true;
                        }
                    }
                }

                if (DoSingle)
                {
                    if (One.Completed)
                    {
                        IntervalFade = Interval;
                        Interval = Interval * 10.0f + 0.25f;
                        Fader();
                    }
                }
                else
                {
                    if (One.Completed && Two.Completed && Three.Completed && Four.Completed && Five.Completed && Six.Completed && Seven.Completed)
                    {
                        IntervalFade = Interval;
                        Interval = Interval * 10.0f + 0.25f;
                        Fader();
                    }
                }
            }
            Wait();
        }

        private void Wait()
        {
            SendCustomEventDelayedSeconds(nameof(StepAll), Interval);
        }

        public void SetList(Vector3 incoming)
        {
            StateList.Add(incoming.ToString());
        }

        public bool ReadList(Vector3 incoming)
        {
            return StateList.Contains(incoming.ToString());
        }

        private bool ShouldWake()
        {
            bool awoken = false;
            int chance = Random.Range(1, 50);

            if (chance == 1)
            {
                awoken = true;
            }

            return awoken;
        }

        public void Fader()
        {
            FadeLevel -= 0.1f;

            if (FadeLevel >= 0.0f)
            {
                SetFade();
                SendCustomEventDelayedSeconds(nameof(Fader), IntervalFade);
            }
            else
            {
                SendCustomEventDelayedSeconds(nameof(FaderEnd), IntervalFade);
            }
        }

        public void FaderEnd()
        {
            FullReset();
            FadeLevel = 1.0f;

            SetFade();

            SendCustomEventDelayedSeconds(nameof(StartPipes), 1.0f);
            Interval = IntervalStored;
        }

        public void SetFade()
        {
            foreach (Material m in Materials)
            {
                Color c = m.GetColor("_Color");
                m.SetColor("_Color", new Color(c.r, c.g, c.b, FadeLevel));
            }
        }

        public void PausePipes()
        {
            IsPaused = !IsPaused;
        }

        public void SetSingle()
        {
            DoSingle = true;
            FullReset();
            IsPaused = false;
        }

        public void SetMultiple()
        {
            DoSingle = false;
            FullReset();
            IsPaused = false;
        }

        public void SetCollide()
        {
            IsColliding = !IsColliding;

            if (IsColliding)
            {
                CollideText.text = "Collision On";
            }
            else
            {
                CollideText.text = "Collision Off";
            }

            One.Collide(IsColliding);
            Two.Collide(IsColliding);
            Three.Collide(IsColliding);
            Four.Collide(IsColliding);
            Five.Collide(IsColliding);
            Six.Collide(IsColliding);
            Seven.Collide(IsColliding);
        }

        public void SetElbow() // TMP Dropdowns aren't exposed, have to use default ones
        {
            BendType = "ball";
            FullReset();
        }

        public void SetBall()
        {
            BendType = "mixed";
            FullReset();
        }

        public void SetMixed()
        {
            BendType = "elbow";
            FullReset();
        }

        public void UpdateSpeed()
        {
            Interval = SpeedSlider.value;
            IntervalStored = Interval;
        }

        private void FullReset()
        {
            One.DestroyParts();
            Two.DestroyParts();
            Three.DestroyParts();
            Four.DestroyParts();
            Five.DestroyParts();
            Six.DestroyParts();
            Seven.DestroyParts();

            WakeTwo = false;
            WakeThree = false;
            WakeFour = false;
            WakeFive = false;
            WakeSix = false;
            WakeSeven = false;

            One.Completed = false;
            Two.Completed = false;
            Three.Completed = false;
            Four.Completed = false;
            Five.Completed = false;
            Six.Completed = false;
            Seven.Completed = false;

            One.SetFinalPiece = false;
            Two.SetFinalPiece = false;
            Three.SetFinalPiece = false;
            Four.SetFinalPiece = false;
            Five.SetFinalPiece = false;
            Six.SetFinalPiece = false;
            Seven.SetFinalPiece = false;

            One.Count = 0;
            Two.Count = 0;
            Three.Count = 0;
            Four.Count = 0;
            Five.Count = 0;
            Six.Count = 0;
            Seven.Count = 0;

            StateList.Clear();
        }
    }
}