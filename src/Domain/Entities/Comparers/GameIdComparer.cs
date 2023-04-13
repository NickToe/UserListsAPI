namespace Domain.Entities.Comparers;

public class GameIdComparer : EqualityComparer<Game>
{
    public override bool Equals(Game? left, Game? right)
    {
        if (left == null && right == null)
        {
            return true;
        }
        else if (left == null || right == null)
        {
            return false;
        }
        else if (left.Id == right.Id)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode(Game obj)
    {
        return (obj.Id).GetHashCode();
    }
}
