﻿using System.Text.Json.Serialization;

﻿namespace HttpObservable.Models
﻿{
﻿    /// <summary>
﻿    /// 分页列表，用于表示分页数据
﻿    /// </summary>
﻿    /// <typeparam name="TEntity">列表项的类型</typeparam>
﻿    public class PagedList<TEntity>
﻿    {
﻿        /// <summary>
﻿        /// 当前页码（从1开始）
﻿        /// </summary>
﻿        [JsonPropertyName("page")]
﻿        public int Page { get; set; }

﻿        /// <summary>
﻿        /// 每页显示的记录数
﻿        /// </summary>
﻿        [JsonPropertyName("pageSize")]
﻿        public int PageSize { get; set; }

﻿        /// <summary>
﻿        /// 总记录数
﻿        /// </summary>
﻿        [JsonPropertyName("totalCount")]
﻿        public int TotalCount { get; set; }

﻿        /// <summary>
﻿        /// 总页数
﻿        /// </summary>
﻿        [JsonPropertyName("totalPages")]
﻿        public int TotalPages { get; set; }

﻿        /// <summary>
﻿        /// 当前页的数据项集合
﻿        /// </summary>
﻿        [JsonPropertyName("items")]
﻿        public List<TEntity> Items { get; set; } = new List<TEntity>();

﻿        /// <summary>
﻿        /// 是否有上一页
﻿        /// </summary>
﻿        [JsonPropertyName("hasPrevPages")]
﻿        public bool HasPrevPages => Page > 1;

﻿        /// <summary>
﻿        /// 是否有下一页
﻿        /// </summary>
﻿        [JsonPropertyName("hasNextPages")]
﻿        public bool HasNextPages => Page < TotalPages;

﻿        /// <summary>
﻿        /// 默认构造函数
﻿        /// </summary>
﻿        public PagedList()
﻿        {
﻿        }

﻿        /// <summary>
﻿        /// 使用指定参数创建分页列表
﻿        /// </summary>
﻿        /// <param name="items">当前页的数据项</param>
﻿        /// <param name="totalCount">总记录数</param>
﻿        /// <param name="page">当前页码</param>
﻿        /// <param name="pageSize">每页记录数</param>
﻿        public PagedList(List<TEntity> items, int totalCount, int page, int pageSize)
﻿        {
﻿            Items = items;
﻿            TotalCount = totalCount;
﻿            Page = page;
﻿            PageSize = pageSize;
﻿            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
﻿        }

﻿        /// <summary>
﻿        /// 创建空的分页列表
﻿        /// </summary>
﻿        /// <param name="page">当前页码</param>
﻿        /// <param name="pageSize">每页记录数</param>
﻿        /// <returns>空的分页列表</returns>
﻿        public static PagedList<TEntity> Empty(int page = 1, int pageSize = 10)
﻿        {
﻿            return new PagedList<TEntity>
﻿            {
﻿                Page = page,
﻿                PageSize = pageSize,
﻿                TotalCount = 0,
﻿                TotalPages = 0,
﻿                Items = new List<TEntity>()
﻿            };
﻿        }

﻿        /// <summary>
﻿        /// 从集合创建分页列表
﻿        /// </summary>
﻿        /// <param name="source">源数据集合</param>
﻿        /// <param name="page">当前页码</param>
﻿        /// <param name="pageSize">每页记录数</param>
﻿        /// <returns>分页后的列表</returns>
﻿        public static PagedList<TEntity> Create(IEnumerable<TEntity> source, int page, int pageSize)
﻿        {
﻿            var count = source.Count();
﻿            var items = source
﻿                .Skip((page - 1) * pageSize)
﻿                .Take(pageSize)
﻿                .ToList();

﻿            return new PagedList<TEntity>(items, count, page, pageSize);
﻿        }

﻿        /// <summary>
﻿        /// 将当前分页列表映射为另一种类型的分页列表
﻿        /// </summary>
﻿        /// <typeparam name="TResult">目标类型</typeparam>
﻿        /// <param name="selector">映射函数</param>
﻿        /// <returns>映射后的分页列表</returns>
﻿        public PagedList<TResult> Map<TResult>(Func<TEntity, TResult> selector)
﻿        {
﻿            return new PagedList<TResult>
﻿            {
﻿                Page = Page,
﻿                PageSize = PageSize,
﻿                TotalCount = TotalCount,
﻿                TotalPages = TotalPages,
﻿                Items = Items.Select(selector).ToList()
﻿            };
﻿        }
﻿    }
﻿}