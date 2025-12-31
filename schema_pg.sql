create table if not exists roles
(
    id          serial primary key,
    name        varchar(50) not null,
    description text
);

create unique index if not exists ix_roles_name
    on roles (name);

create table if not exists users
(
    id            serial primary key,
    username      varchar(50)  not null,
    email         varchar(100) not null,
    password_hash text         not null,
    state         varchar(15)  not null,
    created_at    timestamptz  not null default current_timestamp
);

create unique index if not exists ix_users_username
    on users (username);

create unique index if not exists ix_users_email
    on users (email);


create table if not exists user_session
(
    id           serial primary key,
    user_id      integer not null,
    sign_in_time timestamptz not null default now(),
    sign_out_time timestamptz,
    ip_address   varchar(64),
    user_agent   varchar(256),
    is_active    boolean default true,

    constraint fk_user_session_user
        foreign key (user_id)
        references users (id)
        on delete cascade
);

create index if not exists ix_user_session_user_id
    on user_session (user_id);


create table if not exists user_roles
(
    id      serial primary key,
    user_id integer not null,
    role_id integer not null,

    constraint fk_user_roles_user
        foreign key (user_id)
        references users (id)
        on delete cascade,

    constraint fk_user_roles_role
        foreign key (role_id)
        references roles (id)
        on delete cascade,

    constraint uq_user_roles unique (user_id, role_id)
);

create index if not exists ix_user_roles_user_id
    on user_roles (user_id);

create index if not exists ix_user_roles_role_id
    on user_roles (role_id);


create table if not exists tasks
(
    id          serial primary key,
    title       varchar(255) not null,
    description text,
    status      varchar(20) not null default 'Pending',
    priority    varchar(20) not null default 'Medium',

    creator_id  integer not null,
    assignee_id integer,

    created_at  timestamptz not null default current_timestamp,
    due_date    timestamptz,

    constraint fk_tasks_creator
        foreign key (creator_id)
        references users (id),

    constraint fk_tasks_assignee
        foreign key (assignee_id)
        references users (id)
);

create index if not exists ix_tasks_creator_id
    on tasks (creator_id);

create index if not exists ix_tasks_assignee_id
    on tasks (assignee_id);
	
	

create table if not exists chat_message
(
    id          serial primary key,
    room        varchar(50) default 'global'::character varying,
    user_id     integer      not null,
    user_name   varchar(50)  not null,
    content     varchar(250) not null,
    sent_at     timestamp   default CURRENT_TIMESTAMP,
    receiver_id bigint
);

alter table chat_message
    owner to postgres;

create index idx_chat_message_room
    on chat_message (room);

create index idx_chat_message_user_id
    on chat_message (user_id);


