


namespace Game.Development
{

    public partial class Console
    {

        [Command("Spawn")]
        private void Spawn(string definition, float positionx, float positiony)
        {
            World.Entity.Create(Asset.Get(definition), new() { Position = new() { x = positionx, y = positiony }});
        }
    }
}
