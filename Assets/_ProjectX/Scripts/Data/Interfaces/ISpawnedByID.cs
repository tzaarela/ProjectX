namespace Data.Interfaces
{
	public interface ISpawnedByID
	{
		public void SetSpawnedBy(int netID);
		public int GetSpawnedBy();
	}
}