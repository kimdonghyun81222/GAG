namespace _01.Scripts.Data
{
    public interface ISaveable
    {
        // 고유 식별자 (씬 내에서 또는 전역적으로 고유해야 함)
        // string UniqueID { get; set; } // 구현 클래스에서 직접 관리

        // 게임 데이터를 수집하여 GameSaveData 객체에 채워 넣음
        void PopulateSaveData(GameSaveData saveData);

        // GameSaveData 객체로부터 데이터를 로드하여 자신의 상태를 복원
        void LoadFromSaveData(GameSaveData saveData);
    }
}