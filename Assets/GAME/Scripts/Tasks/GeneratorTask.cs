public class GeneratorTask : GameTask {
    public GameLocalTask NumbersTask;


    public override bool OnTaskOpen(Player player) {
        return true;
    }

    public override bool OnTaskFinish(Player player, params object[] data) {
        return true;
    }

    public override void OnTaskClose(Player player) {
    }

    public override void OnTaskOpenClient() {
    }

    public override void OnTaskGUI() {
        base.OnTaskGUI();
        
    }
}