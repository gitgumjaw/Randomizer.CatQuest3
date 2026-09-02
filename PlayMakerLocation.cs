using HutongGames.PlayMaker;

namespace Randomizer.CatQuest3
{
    public static class PlayMakerLocation
    {
        public static string GetKey(Fsm fsm)
        {
            string sceneName = fsm.GameObject.scene.name;
            string gameObjectName = fsm.GameObject.name;
            string fsmName = fsm.Name;
            string stateName = fsm.ActiveStateName;

            return $"{sceneName}|{gameObjectName}|{fsmName}|{stateName}";
        }
    }
}