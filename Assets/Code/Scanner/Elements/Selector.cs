using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
using UnityEngine.WSA;

namespace Scanner {

    class Selector : Element {
        [SerializeField] GameObject regularMain;
        [SerializeField] GameObject activeMain;

        [SerializeField] RegularPolygon triangleLeft;
        [SerializeField] RegularPolygon triangleRight;

        [SerializeField] Rectangle backgroundScreener;

        [SerializeField] GameObject template;

        [SerializeField] bool wraparound;
        [SerializeField] bool allowUnfold;
        [SerializeField] bool foldOnSelection;

        [SerializeField] string[] initialItems;

        public (int index, string caption, object data) Selected => (cyclerIndex, data[cyclerIndex].caption, data[cyclerIndex].data);

        public event Action<int> IndexChanged;
        public int CyclerIndex { 
            get => cyclerIndex; 
            set { 
                if (cyclerIndex!= value) { cyclerIndex = value;  OnIndexUpdated(); } 
            } 
        }

        private void OnIndexUpdated() {
            regularMain.GetComponentInChildren<TMPro.TMP_Text>().text = data[cyclerIndex].caption;
            activeMain .GetComponentInChildren<TMPro.TMP_Text>().text = data[cyclerIndex].caption;
            timeIndexChange = Time.time;
            IndexChanged?.Invoke(this.cyclerIndex);
        }

        public void SetItems(IEnumerable<string> newItems) {
            if (data == null) data = new();
            if (unfolded) Fold();

            data.Clear();
            foreach (var item in newItems) AddItem(item, null);
        }

        public void AddItem(string item, object data) {
            if (this.data == null) this.data = new();
            this.data.Add((item, data));
        }

        List<(string caption, object data)> data;

        float rotationLerpProgress;
        float _rlVel;

        enum SemanticHighlight {
            None,
            Left,
            Right,
            Center,
        }

        int framesHL;
        float timeSHChange;
        float timeIndexChange = -100;

        bool unfolded;


        SemanticHighlight shState;

        private void Start() {
            cyclerIndex = -1; CyclerIndex = 0;
            if (data == null) data = initialItems.Select(i => (i, (object)null)).ToList();
        }

        private void LateUpdate() {
            var oldsH = shState;
            shState = GetSemanticHighlight(); 
            if (shState != oldsH) {
                SemanticStateChanged();
            }

            if (IsHighlighted) framesHL++; else framesHL = 0;
            
            if (IsHighlighted && Input.GetMouseButtonDown(0)) {
                HandleClick();
            }

            var leftFull = shState == SemanticHighlight.Left;
            var rightFull = shState == SemanticHighlight.Right;
            // var centerFull = shState == SemanticHighlight.Center;
            var centerFull = shState > SemanticHighlight.None;

            if (shState == SemanticHighlight.Center) leftFull = rightFull = centerFull = true;


            var tSinceChange = Time.time - timeSHChange;
            if (IsHighlighted && tSinceChange < 0.25f) {
                if (Time.frameCount % 4 < 2) {
                    leftFull = false; 
                    rightFull = false; 
                }
            }

            var tSinceIndexChange = Time.time - timeIndexChange;
            if (tSinceIndexChange < 0.25f) {
                if (Time.frameCount % 4 < 2) {
                    centerFull = false;
                }
            }

            if (tSinceIndexChange > 0.4f && tSinceIndexChange - Time.deltaTime <= 0.4f) {
                if (foldOnSelection) {
                    Fold();
                }
            }

            if (IsHighlighted && framesHL < 11 && Time.frameCount % 4 < 2) centerFull = false;

            triangleLeft.Border = !leftFull;
            triangleRight.Border = !rightFull;
            activeMain.SetActive(centerFull);
            regularMain.SetActive(!centerFull);

            var arrowsDown = shState == SemanticHighlight.Center;
            arrowsDown |= unfolded;
            if (!allowUnfold) arrowsDown = false;

            // rotationLerpProgress = Mathf.MoveTowards(rotationLerpProgress, shState == SemanticHighlight.Center ? 1f : 0f, Time.deltaTime * 3f);
            rotationLerpProgress = Mathf.SmoothDamp(rotationLerpProgress, arrowsDown ? 1f : 0f, ref _rlVel, 0.025f);

            var p = triangleLeft.transform.parent;
            var a = p.localRotation.eulerAngles;
            a.z = rotationLerpProgress * -30;            
            p.localRotation = Quaternion.Euler(a);

            a = p.localPosition; a.y = rotationLerpProgress * 3; p.localPosition= a;

            p = triangleRight.transform.parent;
            a = p.localRotation.eulerAngles;
            a.z = rotationLerpProgress * 30;
            p.localRotation = Quaternion.Euler(a);
            a = p.localPosition; a.y = rotationLerpProgress * 3; p.localPosition = a;

            if (unfolded) {
                if (!IsHighlighted && !AnyChildHighlighted()) {
                    Fold();
                }
            }
        }

        private bool AnyChildHighlighted() {
            foreach (var item in unfoldedItems) if (item.GetComponent<Element>().IsHighlighted) return true;
            return false;
        }

        private void HandleClick() {
            if (shState == SemanticHighlight.Left) TryCycle(-1);
            else if (shState == SemanticHighlight.Right) TryCycle(1);
            else if (shState == SemanticHighlight.Center && allowUnfold) Unfold();            
        }

        List<GameObject> unfoldedItems = new();

        private void Fold() {
            unfolded = false;
            foreach (var item in unfoldedItems) Destroy(item);
            unfoldedItems.Clear();
            backgroundScreener.gameObject.SetActive(false);
        }

        private void Unfold() {
            unfolded = true;
            foreach (var item in unfoldedItems) Destroy(item);
            unfoldedItems.Clear();

            backgroundScreener.Height = data.Count * 30;
            backgroundScreener.transform.localPosition = new Vector3(0, -backgroundScreener.Height / 2 - 15, backgroundScreener.transform.localPosition.z);

            backgroundScreener.gameObject.SetActive(true);

            var y = backgroundScreener.Height / 2 - 15;
            for (var i = 0; i < data.Count; i++) {
                var item = data[i];
                var q = Instantiate(template, backgroundScreener.transform);
                q.GetComponent<CyclerSubItem>().Init(this, i);
                foreach (var c in q.GetComponentsInChildren<TMPro.TMP_Text>(true)) c.text = item.caption;
                unfoldedItems.Add(q);
                q.transform.localPosition = new Vector3(0, y, -0.5f);
                y -= 30;
                q.SetActive(true);
            }
        }

        private void TryCycle(int delta) {
            var id = cyclerIndex + delta;
            if (id < 0) id = wraparound ? data.Count - 1 : 0;
            else if (id >= data.Count) id = wraparound ? 0 : data.Count - 1;
            CyclerIndex = id;
        }

        private void SemanticStateChanged() {
            timeSHChange = Time.time;  
            // framesSH = 0;
        }

        SemanticHighlight GetSemanticHighlight() {
            if (!IsHighlighted) return SemanticHighlight.None;
            if (unfolded) return SemanticHighlight.Center;
            var t = LastCursorLocalPos.x;
            if (t < -0.3f) return SemanticHighlight.Left;
            if (t > 0.3f) return SemanticHighlight.Right;
            return SemanticHighlight.Center;
        }

        float timeLastClick = -1000f;
        private int cyclerIndex = 0;
    }
}