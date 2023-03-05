namespace Scanner {
    public class OnOffBlinker : Blinker {
        protected override void Initialize() { }
        protected override void UpdateGraphics(bool phase) {
            rend.enabled = phase;
        }
    }
}