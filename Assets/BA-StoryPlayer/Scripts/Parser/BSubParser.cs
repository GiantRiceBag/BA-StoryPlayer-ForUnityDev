namespace BAStoryPlayer
{
    public class BSubParser
    {
        protected int weight = 0; // ����Ȩ��
        public BSubParser nextParser;
        public virtual StoryUnit Parse(RawStoryUnit rawStoryUnit, StoryUnit storyUnit = null) { return null; }
        public void SetNextParser(BSubParser next) { nextParser = next; }
    }
}
