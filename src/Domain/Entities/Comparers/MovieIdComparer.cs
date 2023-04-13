namespace Domain.Entities.Comparers;

public class MovieIdComparer : EqualityComparer<Movie>
{
    public override bool Equals(Movie? left, Movie? right)
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

    public override int GetHashCode(Movie obj)
    {
        return (obj.Id).GetHashCode();
    }
}
