using System.Text;

namespace RpgLibrary.AttackData
{
    public class LearnedAttackManagerData
    {
        public List<string> LearnedAttackNames = new();

        public LearnedAttackManagerData() { }

        public LearnedAttackManagerData(LearnedAttackManagerData data)
        {
            LearnedAttackNames = data.LearnedAttackNames;
        }

        public LearnedAttackManagerData Clone()
        {
            return new LearnedAttackManagerData(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine("LearnedAttackManagerData: ");

            foreach (var obj in LearnedAttackNames)
                sb.AppendLine(obj.ToString() + ", ");

            return sb.ToString();
        }
    }
}
