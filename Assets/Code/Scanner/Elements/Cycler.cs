using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace Scanner {

    class Cycler : Element {
        [SerializeField] GameObject regularMain;
        [SerializeField] GameObject activeMain;

        [SerializeField] RegularPolygon triangleLeft;
        [SerializeField] RegularPolygon triangleRight;

        [SerializeField] Rectangle backgroundScreener;

        [SerializeField] GameObject template;

        [SerializeField] bool wraparound;

        public int CyclerIndex { get => cyclerIndex; set { cyclerIndex = value; OnIndexUpdated(); } }

        private void OnIndexUpdated() {
            regularMain.GetComponentInChildren<TMPro.TMP_Text>().text = items[cyclerIndex];
            activeMain .GetComponentInChildren<TMPro.TMP_Text>().text = items[cyclerIndex];
            timeIndexChange = Time.time;
        }

        [SerializeField] List<string> items;

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
            CyclerIndex = 0;
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

            if (IsHighlighted && framesHL < 11 && Time.frameCount % 4 < 2) centerFull = false;

            triangleLeft.Border = !leftFull;
            triangleRight.Border = !rightFull;
            activeMain.SetActive(centerFull);
            regularMain.SetActive(!centerFull);

            var arrowsDown = shState == SemanticHighlight.Center;
            arrowsDown |= unfolded;

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
        }

        private void HandleClick() {
            if (shState == SemanticHighlight.Left) TryCycle(-1);
            else if (shState == SemanticHighlight.Right) TryCycle(1);
            else if (shState == SemanticHighlight.Center) {
                Unfold();
            }
        }

        List<GameObject> unfoldedItems = new();

        private void Unfold() {
            unfolded = true;
            foreach (var item in unfoldedItems) Destroy(item);
            unfoldedItems.Clear();

            backgroundScreener.Height = items.Count * 30;
            backgroundScreener.transform.localPosition = new Vector3(0, -backgroundScreener.Height / 2 - 15, -1);

            backgroundScreener.gameObject.SetActive(true);

            var y = backgroundScreener.Height / 2 - 15;
            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                var q = Instantiate(template, backgroundScreener.transform);
                q.GetComponent<CyclerSubItem>().Init(this, i);
                foreach (var c in q.GetComponentsInChildren<TMPro.TMP_Text>(true)) c.text = item;
                unfoldedItems.Add(q);
                q.transform.localPosition = new Vector3(0, y, 0);
                y -= 30;
                q.SetActive(true);
            }
        }

        private void TryCycle(int delta) {
            var id = cyclerIndex + delta;
            if (id < 0) id = wraparound ? items.Count - 1 : 0;
            else if (id >= items.Count) id = wraparound ? 0 : items.Count - 1;
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

        //private bool VisualState() {

        //    if (timeLastClick > Time.time - 0.3f) {
        //        return Time.frameCount % 4 < 2;
        //    }

        //    if (IsHighlighted) {
        //        if (framesHL < 7) {
        //            return framesHL % 2 == 0;
        //        }
        //        return true;
        //    }
        //    return false;
        //}
    }
}