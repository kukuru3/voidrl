using Cysharp.Threading.Tasks;

namespace Scanner.Impl.Responders {
    internal class QuitButton : ButtonResponder {
        protected override void OnButtonClicked() {
            ExecuteAnimations().Forget();
        }

        async UniTask ExecuteAnimations() {
            // something like:
            // var token = UIManager.GetAnimationToken("MainMenu");
            // token.FadeOut(0.4f);
            // token.Move(0, -10, 0.4f)
            // await token.WhenAllComplete();
            await UniTask.Delay(1000);
            GameController.ExitGame();
        }
    }
}
