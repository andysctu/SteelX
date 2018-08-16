public class BlockInputSet{
    public enum Elements { ExitingGame, OnEscPanel, GameEnding, OnTyping, PlayerIsDead };
    private bool[] bools;

    public BlockInputSet() {
        bools = new bool[System.Enum.GetValues(typeof(Elements)).Length];

        for(int i = 0; i < bools.Length; i++) {
            bools[i] = false;
        }
    }

    public void SetElement(Elements element, bool b) {
        bools[(int)element] = b;
    }

    public bool IsInputBlocked() {
        for (int i = 0; i < bools.Length; i++) {
            if(bools[i] == true) {
                return true;
            }
        }

        return false;
    }
    
}