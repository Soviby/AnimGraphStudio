namespace Sirenix.OdinInspector.Custom
{
    using System;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public class OdinAssetListAttribute : Attribute
    {
        /// <summary>
        /// <para>If <c>true</c>, all assets found and displayed by the asset list, will automatically be added to the list when inspected.</para>
        /// </summary>
        public bool AutoPopulate;

        /// <summary>
        /// <para>Comma separated list of tags to filter the asset list.</para>
        /// </summary>
        public string Tags;

        /// <summary>
        /// <para>Filter the asset list to only include assets with a specified layer.</para>
        /// </summary>
        public string LayerNames;

        /// <summary>
        /// <para>Filter the asset list to only include assets which name begins with.</para>
        /// </summary>
        public string AssetNamePrefix;

        /// <summary>
        /// <para>Filter the asset list to only include assets which is located at the specified path.</para>
        /// </summary>
        public string Path;

        /// <summary>
        /// <para>Filter the asset list to only include assets for which the given filter method returns true.</para>
        /// </summary>
        public string CustomFilterMethod;

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="AssetListAttribute"/> class.</para>
        /// </summary>
        public OdinAssetListAttribute()
        {
            this.AutoPopulate = false;
            this.Tags = null;
            this.LayerNames = null;
            this.AssetNamePrefix = null;
            this.CustomFilterMethod = null;
        }
    }
}