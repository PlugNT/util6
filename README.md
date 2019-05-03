<p>
    # Util6 ORM
</p>
<p>
    <br/>
</p>
<p>
    <span style="font-size: 12px;"><strong><span style="color:blue;"></span></strong></span>
</p>
<p>
    Util6 ORM 创建于2014年，于2019年正式开源，是在Util.Database数据库框架依赖的基础上实现的一个轻量级强类型ORM框架。
</p>
<p>
    官方网站：https://www.util6.com
</p>
<p>
    <span style="font-size: 12px;"></span><br/>
</p>
<h4>
    Util6 ORM 特点
</h4>
<ul style="list-style-type: square;" class=" list-paddingleft-2">
    <li>
        <p>
            <span style="font-size: 12px;">上手容易，语法简单，<span style="font-size: 12px;">性能优，体积小</span></span>
        </p>
    </li>
    <li>
        <p>
            <span style="font-size: 12px;">核心方法：查询(<span style="font-size: 12px;">Query</span>) 、新增(Insert)、更新(Update)、删除(Delete)</span>
        </p>
    </li>
    <li>
        <p>
            <span style="font-size: 12px;">支持多数据库<span style="font-size: 12px; color: rgb(0, 112, 192);">Sql Server</span>,<span style="font-size: 12px; color: rgb(0, 112, 192);">MySql</span>,<span style="font-size: 12px; color: rgb(0, 112, 192);">Access</span>,等数据库</span>
        </p>
    </li>
</ul>
<p>
    <span style="font-size: 12px;"></span>
</p>
<ul class=" list-paddingleft-2" style="list-style-type: square;">
    <li>
        <p>
            <span style="font-size: 12px;">支持数据库读写分离配置</span>
        </p>
    </li>
</ul>
<p>
    <span style="font-size: 12px;"></span><br/>
</p>
<p></p>
<pre><span style="color: #0000ff;">public</span> FindMaintainRoleOutput FindMaintainRole(<span style="color: #0000ff;">int</span><span style="color: #000000;"> roleID)
{
    </span><span style="color: #0000ff;">var</span> output = <span style="color: #0000ff;">new</span><span style="color: #000000;"> FindMaintainRoleOutput();
    output.Role </span>= <span style="color: #0000ff;">new</span> MisRoleDataAccess().Query(m =&gt; m.ID ==<span style="color: #000000;"> roleID).ToModel();
    </span><span style="color: #0000ff;">if</span> (output.Role != <span style="color: #0000ff;">null</span><span style="color: #000000;">)
    {
        </span><span style="color: #0000ff;">var</span> role =<span style="color: #000000;"> output.Role;
        output.Project </span>= <span style="color: #0000ff;">new</span><span style="color: #000000;"> MisProjectService().FindEntity(role.ProjectID);
        output.UserRoleList </span>= <span style="color: #0000ff;">new</span> MisUserRoleDataAccess().Query(m =&gt; m.RoleID ==<span style="color: #000000;"> role.ID).ToList();
        output.RoleRightsList </span>= <span style="color: #0000ff;">new</span> MisRoleRightsDataAccess().Query(m =&gt; m.RoleID ==<span style="color: #000000;"> role.ID).ToList();
        output.RoleExtendList </span>= <span style="color: #0000ff;">new</span> MisRoleExtendDataAccess().Query(m =&gt; m.RoleID ==<span style="color: #000000;"> role.ID).ToList();
        </span><span style="color: #0000ff;">var</span> rightsIDList = output.RoleRightsList.Select(m =&gt;<span style="color: #000000;"> m.RightsID).ToList();
        </span><span style="color: #0000ff;">if</span> (rightsIDList.Count &gt; <span style="color: #800080;">0</span><span style="color: #000000;">)
        {
            output.RightsList </span>= <span style="color: #0000ff;">new</span> MisRightsDataAccess().Query(m=&gt;<span style="color: #000000;"> rightsIDList.Contains(m.ID)).ToList();
            output.RightsPageList </span>= <span style="color: #0000ff;">new</span> MisRightsPageDataAccess().Query(m =&gt;<span style="color: #000000;"> rightsIDList.Contains(m.RightsID)).ToList();
        }
    }
    </span><span style="color: #0000ff;">return</span><span style="color: #000000;"> output;
}</span></pre>
<p>
    <br/>
</p>
