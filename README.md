敌人 AI 状态机系统：基于泛型 StateMachine<T> 框架实现 Idle → CombatMovement（追击/绕圈）→ Attack → RetreatAfterAttack 状态流转；VisionSensor 触发器 + FOV 角度实现目标感知
多人敌人攻击调度：通过 EnemyManager 统一管理攻击窗口、随机计时与攻击者选择，实现多个敌人"轮流进攻"的战斗节奏
近战战斗系统：连招（Combo）机制、基于骨骼挂点碰撞体的命中判定（武器/手脚 Hitbox）、攻击数据驱动（ScriptableObject 配置动画名、伤害窗口、命中部位）、受击反馈动画
动画系统：Animator 分层（Locomotion 混合树 + 攻击层），forwardSpeed/strafeSpeed 参数驱动，NavMeshAgent 寻路
问题定位与修复：处理了空引用崩溃、命名冲突编译错误、攻击调度逻辑缺陷，以及动画根运动导致的角色横移、URP/HDRP 素材在 Built-in 管线下的兼容问题
